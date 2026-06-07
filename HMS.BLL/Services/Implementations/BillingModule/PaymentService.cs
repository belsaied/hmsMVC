using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;
using HMS.DAL.Models.PatientModule;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HMS.BLL.ServicesAbstraction.Contracts.BillingService;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.BillingModule;
using HMS.BLL.Shared.Dtos.BillingModule.Results;
using HMS.BLL.Shared.Dtos.NotificationDtos.Events;
using Stripe;
using Invoice = HMS.DAL.Models.BillingModule.Invoice;
using PaymentMethod = HMS.DAL.Models.Enums.BillingEnums.PaymentMethod;

namespace Services.Implementations.BillingModule
{
    public sealed class PaymentService (IUnitOfWork _unitOfWork
        , IMapper _mapper, INotificationService _notificationService, IConfiguration _config,ILogger<PaymentService> _logger) : IPaymentService
    {
        // Initialise Stripe once per service lifetime — same pattern as E-Commerce project
        static PaymentService()
        {
            // ApiKey is set in DI registration via StripeSettings:SecretKey — see below.
        }

        // ── Stripe PaymentIntent ──────────────────────────────────────────────

        public async Task<PaymentIntentResultDto> CreatePaymentIntentAsync(Guid invoiceId)
        {
            StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];

            var invoice = await LoadInvoiceAsync(invoiceId);

            if (invoice.Status is InvoiceStatus.Draft or InvoiceStatus.Cancelled)
                throw new DraftInvoicePaymentException(invoiceId);

            if (invoice.OutstandingBalance <= 0)
                throw new BusinessRuleException($"Invoice '{invoiceId}' has no outstanding balance.");

            var pendingSpec = new PaymentsByInvoiceSpecification(invoiceId);
            var existing = (await _unitOfWork.GetRepository<Payment, Guid>().GetAllAsync(pendingSpec))
                .FirstOrDefault(p => p.Status == PaymentStatus.Pending
                                 && !string.IsNullOrEmpty(p.StripePaymentIntentId));

            if (existing is not null)
            {
                return new PaymentIntentResultDto
                {
                    InvoiceId = invoiceId,
                    StripePaymentIntentId = existing.StripePaymentIntentId!,
                    ClientSecret = existing.StripeClientSecret!,
                    Amount = invoice.OutstandingBalance
                };
            }

            // Stripe requires amounts in the smallest currency unit (cents for USD)
            var amountInCents = (long)(invoice.OutstandingBalance * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = "usd",
                Metadata = new Dictionary<string, string>
                {
                    ["invoiceId"] = invoiceId.ToString(),
                    ["patientId"] = invoice.PatientId.ToString()
                }
            };

            var intentService = new PaymentIntentService();
            var intent = await intentService.CreateAsync(options);

            var payment = new Payment
            {
                InvoiceId = invoiceId,
                PatientId = invoice.PatientId,
                Amount = invoice.OutstandingBalance,
                PaymentMethod = PaymentMethod.Card,
                Status = PaymentStatus.Pending,
                StripePaymentIntentId = intent.Id,
                StripeClientSecret = intent.ClientSecret
            };

            await _unitOfWork.GetRepository<Payment, Guid>().AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return new PaymentIntentResultDto
            {
                InvoiceId = invoiceId,
                StripePaymentIntentId = intent.Id,
                ClientSecret = intent.ClientSecret,
                Amount = invoice.OutstandingBalance
            };
        }

        // ── Cash Payment ──────────────────────────────────────────────────────

        public async Task<PaymentResultDto> RecordCashPaymentAsync(Guid invoiceId, decimal amount)
        {
            var invoice = await LoadInvoiceAsync(invoiceId);

            ValidatePaymentCanProceed(invoice, amount);

            var payment = new Payment
            {
                InvoiceId = invoiceId,
                PatientId = invoice.PatientId,
                Amount = amount,
                PaymentMethod = PaymentMethod.Cash,
                Status = PaymentStatus.Succeeded,
                TransactionReference = $"CASH-{Guid.NewGuid():N}".ToUpperInvariant(),
                PaidAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.GetRepository<Payment, Guid>().AddAsync(payment);
            ApplyPaymentToInvoice(invoice, amount);
            _unitOfWork.GetRepository<Invoice, Guid>().Update(invoice);
            await _unitOfWork.SaveChangesAsync();
            try
            {
                var patient = await _unitOfWork.GetRepository<Patient, int>().GetByIdAsync(payment.PatientId);
                await _notificationService.SendPaymentReceivedAsync(new PaymentNotificationEvent
                {
                    PaymentId = payment.Id,
                    InvoiceId = payment.InvoiceId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    PatientId = payment.PatientId,
                    PatientEmail = patient?.Email ?? string.Empty,
                    PatientName = patient is not null ? $"{patient.FirstName} {patient.LastName}" : "Unknown",
                    AmountPaid = payment.Amount,
                    PaidAt = payment.PaidAt ?? DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Notification] Failed to send PaymentReceived for payment {Id}", payment.Id);
            }
            return _mapper.Map<PaymentResultDto>(payment);
        }

        // ── Stripe Webhook ────────────────────────────────────────────────────

        public async Task HandleStripeWebhookAsync(string payload, string stripeSignature)
        {
            StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];

            var webhookSecret = _config["StripeSettings:WebhookSecret"]
                                ?? throw new InvalidOperationException("StripeSettings:WebhookSecret is not configured.");

            Stripe.Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(payload, stripeSignature, webhookSecret);
            }
            catch (StripeException ex)
            {
                throw new BusinessRuleException($"Stripe webhook validation failed: {ex.Message}");
            }

            switch (stripeEvent.Type)
            {
                case EventTypes.PaymentIntentSucceeded:
                    await HandleSucceededAsync((PaymentIntent)stripeEvent.Data.Object);
                    break;

                case EventTypes.PaymentIntentPaymentFailed:
                    await HandleFailedAsync((PaymentIntent)stripeEvent.Data.Object);
                    break;
            }
        }

        private async Task HandleSucceededAsync(PaymentIntent intent)
        {
            var spec = new PaymentByStripeIntentSpecification(intent.Id);
            var payment = await _unitOfWork.GetRepository<Payment, Guid>().GetByIdAsync(spec);
            if (payment is null) return; // Idempotent — already processed

            if (payment.Status == PaymentStatus.Succeeded) return; // Already handled

            payment.Status = PaymentStatus.Succeeded;
            payment.PaidAt = DateTimeOffset.UtcNow;

            var invoice = await LoadInvoiceAsync(payment.InvoiceId);
            ApplyPaymentToInvoice(invoice, payment.Amount);

            _unitOfWork.GetRepository<Payment, Guid>().Update(payment);
            _unitOfWork.GetRepository<Invoice, Guid>().Update(invoice);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                var patient = await _unitOfWork.GetRepository<Patient, int>().GetByIdAsync(payment.PatientId);
                await _notificationService.SendPaymentReceivedAsync(new PaymentNotificationEvent
                {
                    PaymentId = payment.Id,
                    InvoiceId = payment.InvoiceId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    PatientId = payment.PatientId,
                    PatientEmail = patient?.Email ?? string.Empty,
                    PatientName = patient is not null ? $"{patient.FirstName} {patient.LastName}" : "Unknown",
                    AmountPaid = payment.Amount,
                    PaidAt = payment.PaidAt ?? DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Notification] Failed to send PaymentReceived for payment {Id}", payment.Id);
            }
        }

        private async Task HandleFailedAsync(PaymentIntent intent)
        {
            var spec = new PaymentByStripeIntentSpecification(intent.Id);
            var payment = await _unitOfWork.GetRepository<Payment, Guid>().GetByIdAsync(spec);
            if (payment is null) return;

            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = intent.LastPaymentError?.Message ?? "Unknown failure.";

            _unitOfWork.GetRepository<Payment, Guid>().Update(payment);
            await _unitOfWork.SaveChangesAsync();
        }

        // ── Refund ────────────────────────────────────────────────────────────

        public async Task<PaymentResultDto> RefundPaymentAsync(Guid paymentId, decimal amount, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessRuleException("A refund reason is required.");

            var payment = await _unitOfWork.GetRepository<Payment, Guid>().GetByIdAsync(paymentId)
                          ?? throw new PaymentNotFoundException(paymentId);

            if (payment.Status != PaymentStatus.Succeeded)
                throw new BusinessRuleException("Only Succeeded payments can be refunded.");

            if (amount <= 0 || amount > payment.Amount)
                throw new BusinessRuleException(
                    $"Refund amount must be between 0.01 and {payment.Amount:N2}.");

            // Issue Stripe refund for card payments
            if (payment.PaymentMethod == HMS.DAL.Models.Enums.BillingEnums.PaymentMethod.Card
                && !string.IsNullOrEmpty(payment.StripePaymentIntentId))
            {
                StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = payment.StripePaymentIntentId,
                    Amount = (long)(amount * 100),
                    Reason = RefundReasons.RequestedByCustomer
                };
                var refundService = new RefundService();
                await refundService.CreateAsync(refundOptions);
            }

            payment.Status = PaymentStatus.Refunded;
            payment.RefundedAt = DateTimeOffset.UtcNow;
            payment.RefundReason = reason;

            // Roll back the invoice's PaidAmount
            var invoice = await LoadInvoiceAsync(payment.InvoiceId);
            invoice.PaidAmount = Math.Max(0, invoice.PaidAmount - amount);
            ReevaluateInvoiceStatus(invoice);

            _unitOfWork.GetRepository<Payment, Guid>().Update(payment);
            _unitOfWork.GetRepository<Invoice, Guid>().Update(invoice);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PaymentResultDto>(payment);
        }

        // ── Query ─────────────────────────────────────────────────────────────

        public async Task<IEnumerable<PaymentResultDto>> GetPaymentsByInvoiceAsync(Guid invoiceId)
        {
            _ = await _unitOfWork.GetRepository<Invoice, Guid>().GetByIdAsync(invoiceId)
                ?? throw new InvoiceNotFoundException(invoiceId);

            var spec = new PaymentsByInvoiceSpecification(invoiceId);
            var payments = await _unitOfWork.GetRepository<Payment, Guid>().GetAllAsync(spec);
            return _mapper.Map<IEnumerable<PaymentResultDto>>(payments);
        }

        // ── Private Helpers ───────────────────────────────────────────────────

        private async Task<HMS.DAL.Models.BillingModule.Invoice> LoadInvoiceAsync(Guid invoiceId)
        {
            var spec = new InvoiceWithDetailsSpecification(invoiceId);
            return await _unitOfWork.GetRepository<HMS.DAL.Models.BillingModule.Invoice, Guid>().GetByIdAsync(spec)
                   ?? throw new InvoiceNotFoundException(invoiceId);
        }

        private static void ValidatePaymentCanProceed(HMS.DAL.Models.BillingModule.Invoice invoice, decimal amount)
        {
            if (invoice.Status == InvoiceStatus.Draft)
                throw new DraftInvoicePaymentException(invoice.Id);

            if (invoice.Status == InvoiceStatus.Cancelled)
                throw new InvalidInvoiceStatusTransitionException(invoice.Status.ToString(), "Paid");

            if (amount <= 0)
                throw new BusinessRuleException("Payment amount must be greater than zero.");

            if (amount > invoice.OutstandingBalance)
                throw new PaymentExceedsBalanceException(amount, invoice.OutstandingBalance);
        }

        private static void ApplyPaymentToInvoice(HMS.DAL.Models.BillingModule.Invoice invoice, decimal amount)
        {
            invoice.PaidAmount += amount;
            ReevaluateInvoiceStatus(invoice);
        }

        private static void ReevaluateInvoiceStatus(HMS.DAL.Models.BillingModule.Invoice invoice)
        {
            invoice.RecalculateFinancials();

            if (invoice.PaidAmount >= invoice.TotalAmount)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidAt = DateTimeOffset.UtcNow;
            }
            else if (invoice.PaidAmount > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
                invoice.PaidAt = null;
            }
            else
            {
                if (invoice.Status == InvoiceStatus.Paid)
                    invoice.Status = InvoiceStatus.Issued;
                invoice.PaidAt = null;
            }
        }
    }
}

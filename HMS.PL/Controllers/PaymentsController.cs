using HMS.BLL.Services.Exceptions;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.PL.ViewModels.BillingModule;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace HMS.PL.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IServiceManager _services;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IServiceManager services, IConfiguration configuration, ILogger<PaymentsController> logger)
        {
            _services = services;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> RecordCash(Guid invoiceId)
        {
            try
            {
                var invoice = await _services.InvoiceService.GetInvoiceByIdAsync(invoiceId);
                return View(new RecordCashViewModel
                {
                    InvoiceId = invoiceId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    OutstandingBalance = invoice.OutstandingBalance
                });
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Invoice not found.";
                return RedirectToAction("Index", "Invoices");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordCash(RecordCashViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                await _services.PaymentService.RecordCashPaymentAsync(vm.InvoiceId, vm.Amount);
                TempData["Success"] = $"Cash payment of {vm.Amount:C} recorded successfully.";
                return RedirectToAction("Details", "Invoices", new { id = vm.InvoiceId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        public async Task<IActionResult> StripeCheckout(Guid invoiceId)
        {
            try
            {
                var invoice = await _services.InvoiceService.GetInvoiceByIdAsync(invoiceId);
                var intent = await _services.PaymentService.CreatePaymentIntentAsync(invoiceId);

                ViewBag.ClientSecret = intent.ClientSecret;
                ViewBag.Amount = intent.Amount;
                ViewBag.InvoiceNumber = invoice.InvoiceNumber;
                ViewBag.InvoiceId = invoiceId;
                ViewBag.StripePublicKey = _configuration["StripeSettings:PublicKey"];

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Could not initiate Stripe payment: " + ex.Message;
                return RedirectToAction("Details", "Invoices", new { id = invoiceId });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> StripeWebhook()
        {
            Request.EnableBuffering();
            Request.Body.Position = 0;
            string payload;
            using (var reader = new StreamReader(Request.Body, leaveOpen: true))
            {
                payload = await reader.ReadToEndAsync();
            }

            var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
            if (string.IsNullOrEmpty(signature))
                return BadRequest("Missing Stripe-Signature header.");

            try
            {
                await _services.PaymentService.HandleStripeWebhookAsync(payload, signature);
                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook validation failed for event");
                return BadRequest($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in StripeWebhook");
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> Refund(Guid paymentId, Guid invoiceId)
        {
            try
            {
                var payments = await _services.PaymentService.GetPaymentsByInvoiceAsync(invoiceId);
                var payment = payments.FirstOrDefault(p => p.Id == paymentId);

                if (payment is null)
                {
                    TempData["Error"] = "Payment not found.";
                    return RedirectToAction("Details", "Invoices", new { id = invoiceId });
                }

                return View(new RefundViewModel
                {
                    PaymentId = paymentId,
                    InvoiceId = invoiceId,
                    PaidAmount = payment.Amount
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", "Invoices", new { id = invoiceId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(RefundViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                await _services.PaymentService.RefundPaymentAsync(vm.PaymentId, vm.Amount, vm.Reason);
                TempData["Success"] = $"Refund of {vm.Amount:C} processed successfully.";
                return RedirectToAction("Details", "Invoices", new { id = vm.InvoiceId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }
    }
}

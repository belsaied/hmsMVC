using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;
using HMS.BLL.ServicesAbstraction.Contracts.BillingService;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.BillingModule;
using HMS.BLL.Shared.Dtos.BillingModule.Requests;
using HMS.BLL.Shared.Dtos.BillingModule.Results;

namespace HMS.BLL.Services.Implementations.BillingModule
{
    public class InsuranceService (IUnitOfWork _unitOfWork , IMapper _mapper) : IInsuranceService
    {
        // ── Submit ────────────────────────────────────────────────────────────

        public async Task<ClaimResultDto> SubmitClaimAsync(SubmitClaimRequest request)
        {
            var invoice = await _unitOfWork.GetRepository<Invoice, Guid>().GetByIdAsync(request.InvoiceId)
                          ?? throw new InvoiceNotFoundException(request.InvoiceId);

            // One active claim per invoice
            var existingSpec = new ClaimByInvoiceSpecification(request.InvoiceId);
            var existing = await _unitOfWork.GetRepository<InsuranceClaim, Guid>().GetByIdAsync(existingSpec);
            if (existing is not null)
                throw new DuplicateInsuranceClaimException(request.InvoiceId);

            if (string.IsNullOrWhiteSpace(request.InsuranceProvider))
                throw new BusinessRuleException("InsuranceProvider is required.");

            if (string.IsNullOrWhiteSpace(request.PolicyNumber))
                throw new BusinessRuleException("PolicyNumber is required.");

            var claim = new InsuranceClaim
            {
                InvoiceId = request.InvoiceId,
                PatientId = invoice.PatientId,
                InsuranceProvider = request.InsuranceProvider,
                PolicyNumber = request.PolicyNumber,
                MembershipNumber = request.MembershipNumber,
                ClaimedAmount = request.ClaimedAmount,
                Notes = request.Notes,
                ClaimStatus = ClaimStatus.Submitted,
                SubmittedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.GetRepository<InsuranceClaim, Guid>().AddAsync(claim);
            await _unitOfWork.SaveChangesAsync();

            // Reload with Invoice navigation for PatientCopayment calculation
            var reloaded = await LoadClaimWithInvoiceAsync(claim.Id);
            return _mapper.Map<ClaimResultDto>(reloaded);
        }

        // ── Query ─────────────────────────────────────────────────────────────

        public async Task<ClaimResultDto> GetClaimByInvoiceAsync(Guid invoiceId)
        {
            var spec = new ClaimByInvoiceSpecification(invoiceId);
            var claim = await _unitOfWork.GetRepository<InsuranceClaim, Guid>().GetByIdAsync(spec)
                        ?? throw new InsuranceClaimNotFoundException(invoiceId);

            return _mapper.Map<ClaimResultDto>(claim);
        }

        // ── Update Status ─────────────────────────────────────────────────────

        public async Task<ClaimResultDto> UpdateClaimStatusAsync(Guid claimId, UpdateClaimRequest request)
        {
            var claim = await LoadClaimWithInvoiceAsync(claimId);

            claim.ClaimStatus = request.ClaimStatus;
            claim.ResolvedAt = DateTimeOffset.UtcNow;

            switch (request.ClaimStatus)
            {
                case ClaimStatus.Approved:
                case ClaimStatus.PartiallyApproved:
                    {
                        var approved = request.ApprovedAmount
                                       ?? throw new BusinessRuleException("ApprovedAmount is required when approving a claim.");

                        if (approved > claim.ClaimedAmount)
                            throw new BusinessRuleException("ApprovedAmount cannot exceed ClaimedAmount.");

                        claim.ApprovedAmount = approved;

                        // Auto-create insurance payment record
                        var insurancePayment = new Payment
                        {
                            InvoiceId = claim.InvoiceId,
                            PatientId = claim.PatientId,
                            Amount = approved,
                            PaymentMethod = PaymentMethod.InsuranceCover,
                            Status = PaymentStatus.Succeeded,
                            TransactionReference = $"INS-{claimId:N}".ToUpperInvariant(),
                            PaidAt = DateTimeOffset.UtcNow
                        };
                        await _unitOfWork.GetRepository<Payment, Guid>().AddAsync(insurancePayment);

                        // Update invoice PaidAmount
                        claim.Invoice.PaidAmount += approved;
                        if (claim.Invoice.PaidAmount >= claim.Invoice.TotalAmount)
                        {
                            claim.Invoice.Status = InvoiceStatus.Paid;
                            claim.Invoice.PaidAt = DateTimeOffset.UtcNow;
                        }
                        else if (claim.Invoice.PaidAmount > 0)
                        {
                            claim.Invoice.Status = InvoiceStatus.PartiallyPaid;
                        }
                        _unitOfWork.GetRepository<Invoice, Guid>().Update(claim.Invoice);
                        break;
                    }

                case ClaimStatus.Rejected:
                    if (string.IsNullOrWhiteSpace(request.RejectionReason))
                        throw new BusinessRuleException("RejectionReason is required when rejecting a claim.");
                    claim.RejectionReason = request.RejectionReason;
                    break;
            }

            _unitOfWork.GetRepository<InsuranceClaim, Guid>().Update(claim);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClaimResultDto>(claim);
        }

        // ── Resubmit ──────────────────────────────────────────────────────────

        public async Task<ClaimResultDto> ResubmitClaimAsync(Guid claimId, ResubmitClaimRequest request)
        {
            var claim = await LoadClaimWithInvoiceAsync(claimId);

            if (claim.ClaimStatus != ClaimStatus.Rejected)
                throw new ClaimNotRejectedException(claimId);

            claim.InsuranceProvider = request.InsuranceProvider;
            claim.PolicyNumber = request.PolicyNumber;
            claim.MembershipNumber = request.MembershipNumber;
            claim.ClaimedAmount = request.ClaimedAmount;
            claim.Notes = request.Notes;
            claim.ClaimStatus = ClaimStatus.Resubmitted;
            claim.RejectionReason = null;
            claim.ResolvedAt = null;
            claim.SubmittedAt = DateTimeOffset.UtcNow;

            _unitOfWork.GetRepository<InsuranceClaim, Guid>().Update(claim);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClaimResultDto>(claim);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private async Task<InsuranceClaim> LoadClaimWithInvoiceAsync(Guid claimId)
        {
            var spec = new ClaimWithInvoiceSpecification(claimId);
            return await _unitOfWork.GetRepository<InsuranceClaim, Guid>().GetByIdAsync(spec)
                   ?? throw new NotFoundException($"Insurance claim with Id '{claimId}' was not found.");
        }
    }
}

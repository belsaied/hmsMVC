using HMS.BLL.Services.Exceptions;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Dtos.BillingModule.Requests;
using HMS.DAL.Models.Enums.BillingEnums;
using HMS.PL.ViewModels.BillingModule;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HMS.PL.Controllers
{
    public class InsuranceController : Controller
    {
        private readonly IServiceManager _services;

        public InsuranceController(IServiceManager services)
        {
            _services = services;
        }

        public async Task<IActionResult> Submit(Guid invoiceId)
        {
            try
            {
                var invoice = await _services.InvoiceService.GetInvoiceByIdAsync(invoiceId);
                return View(new SubmitClaimViewModel
                {
                    InvoiceId = invoiceId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    TotalAmount = invoice.TotalAmount,
                    ClaimedAmount = invoice.TotalAmount
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
        public async Task<IActionResult> Submit(SubmitClaimViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var request = new SubmitClaimRequest
                {
                    InvoiceId = vm.InvoiceId,
                    InsuranceProvider = vm.InsuranceProvider,
                    PolicyNumber = vm.PolicyNumber,
                    MembershipNumber = vm.MembershipNumber,
                    ClaimedAmount = vm.ClaimedAmount,
                    Notes = vm.Notes
                };

                await _services.InsuranceService.SubmitClaimAsync(request);
                TempData["Success"] = "Insurance claim submitted successfully.";
                return RedirectToAction(nameof(ClaimDetails), new { invoiceId = vm.InvoiceId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        public async Task<IActionResult> ClaimDetails(Guid invoiceId)
        {
            try
            {
                var claim = await _services.InsuranceService.GetClaimByInvoiceAsync(invoiceId);
                ViewBag.InvoiceId = invoiceId;
                return View(claim);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "No insurance claim found for this invoice.";
                return RedirectToAction("Details", "Invoices", new { id = invoiceId });
            }
        }

        public async Task<IActionResult> UpdateStatus(Guid claimId, Guid invoiceId)
        {
            try
            {
                var claim = await _services.InsuranceService.GetClaimByInvoiceAsync(invoiceId);
                PopulateClaimStatusDropdown();
                return View(new UpdateClaimViewModel
                {
                    ClaimId = claimId,
                    InvoiceId = invoiceId,
                    ClaimedAmount = claim.ClaimedAmount
                });
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Index", "Invoices");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(UpdateClaimViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                PopulateClaimStatusDropdown();
                return View(vm);
            }

            try
            {
                var request = new UpdateClaimRequest
                {
                    ClaimStatus = vm.ClaimStatus,
                    ApprovedAmount = vm.ApprovedAmount,
                    RejectionReason = vm.RejectionReason
                };

                await _services.InsuranceService.UpdateClaimStatusAsync(vm.ClaimId, request);
                TempData["Success"] = $"Claim status updated to {vm.ClaimStatus}.";
                return RedirectToAction(nameof(ClaimDetails), new { invoiceId = vm.InvoiceId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                PopulateClaimStatusDropdown();
                return View(vm);
            }
        }

        public async Task<IActionResult> Resubmit(Guid claimId, Guid invoiceId)
        {
            try
            {
                var claim = await _services.InsuranceService.GetClaimByInvoiceAsync(invoiceId);

                if (claim.ClaimStatus != ClaimStatus.Rejected)
                {
                    TempData["Error"] = "Only rejected claims can be resubmitted.";
                    return RedirectToAction(nameof(ClaimDetails), new { invoiceId });
                }

                return View(new ResubmitClaimViewModel
                {
                    ClaimId = claimId,
                    InvoiceId = invoiceId,
                    InsuranceProvider = claim.InsuranceProvider,
                    PolicyNumber = claim.PolicyNumber,
                    MembershipNumber = claim.MembershipNumber,
                    ClaimedAmount = claim.ClaimedAmount
                });
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Index", "Invoices");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resubmit(ResubmitClaimViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var request = new ResubmitClaimRequest
                {
                    InsuranceProvider = vm.InsuranceProvider,
                    PolicyNumber = vm.PolicyNumber,
                    MembershipNumber = vm.MembershipNumber,
                    ClaimedAmount = vm.ClaimedAmount,
                    Notes = vm.Notes
                };

                await _services.InsuranceService.ResubmitClaimAsync(vm.ClaimId, request);
                TempData["Success"] = "Claim resubmitted successfully.";
                return RedirectToAction(nameof(ClaimDetails), new { invoiceId = vm.InvoiceId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        private void PopulateClaimStatusDropdown(string? selected = null)
        {
            ViewBag.ClaimStatusList = new SelectList(
                Enum.GetValues<ClaimStatus>()
                    .Select(v => new { Value = (int)v, Text = v.ToString() }),
                "Value", "Text", selected);
        }
    }
}

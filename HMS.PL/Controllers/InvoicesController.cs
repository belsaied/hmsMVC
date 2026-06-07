using HMS.BLL.Services.Exceptions;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Dtos.BillingModule.Requests;
using HMS.BLL.Shared.Parameters;
using HMS.DAL.Models.Enums.BillingEnums;
using HMS.PL.ViewModels.BillingModule;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HMS.PL.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly IServiceManager _services;

        public InvoicesController(IServiceManager services)
        {
            _services = services;
        }

        public async Task<IActionResult> Index(
            InvoiceStatus? status = null,
            int? patientId = null,
            int pageIndex = 1,
            int pageSize = 10)
        {
            try
            {
                var filters = new InvoiceFilterParameters
                {
                    Status = status,
                    PatientId = patientId,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };

                var result = await _services.InvoiceService.GetAllInvoicesAsync(filters);

                var vm = new InvoiceIndexViewModel
                {
                    Invoices = result.Data,
                    TotalCount = result.TotalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    StatusFilter = status,
                    PatientIdFilter = patientId
                };

                ViewBag.StatusList = new SelectList(
                    Enum.GetNames(typeof(InvoiceStatus)), status?.ToString());

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load invoices: " + ex.Message;
                return View(new InvoiceIndexViewModel());
            }
        }

        public async Task<IActionResult> Details(Guid id, string? redirect_status)
        {
            try
            {
                var invoice = await _services.InvoiceService.GetInvoiceByIdAsync(id);

                if (!string.IsNullOrEmpty(redirect_status))
                {
                    if (redirect_status == "succeeded")
                        TempData["Success"] = "Card payment successful! Your invoice has been updated.";
                    else if (redirect_status == "failed")
                        TempData["Error"] = "Payment failed. Please try again or use a different card.";
                }

                return View(invoice);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Invoice not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create(int? patientId, int? appointmentId)
        {
            PopulateLineItemTypeDropdown();

            var vm = new CreateInvoiceViewModel
            {
                PatientId = patientId ?? 0,
                AppointmentId = appointmentId
            };

            if (patientId.HasValue)
            {
                var autoItems = new List<LineItemEntryViewModel>();

                // 1. Consultation fee from appointment
                if (appointmentId.HasValue)
                {
                    try
                    {
                        var appt = await _services.AppointmentService.GetAppointmentByIdAsync(appointmentId.Value);
                        if (appt != null)
                        {
                            decimal consultationFee = 0m;
                            try
                            {
                                var doctor = await _services.DoctorService.GetDoctorByIdAsync(appt.DoctorId);
                                consultationFee = doctor.ConsultationFee;
                            }
                            catch { }

                            autoItems.Add(new LineItemEntryViewModel
                            {
                                Description = $"Consultation — Dr. {appt.DoctorName} ({appt.AppointmentDate:dd MMM yyyy})",
                                LineItemType = LineItemType.Consultation,
                                ReferenceId = appointmentId.Value.ToString(),
                                Quantity = 1,
                                UnitPrice = consultationFee,
                                IsAutoAdded = true
                            });
                        }
                    }
                    catch { }
                }

                // 2. Completed lab orders not yet billed
                try
                {
                    var labOrders = await _services.LabOrderService.GetPatientLabOrdersAsync(patientId.Value);
                    foreach (var lo in labOrders.Where(l => l.Status == "Completed"))
                    {
                        autoItems.Add(new LineItemEntryViewModel
                        {
                            Description = $"Lab: {lo.TestName}",
                            LineItemType = LineItemType.LabTest,
                            ReferenceId = lo.Id.ToString(),
                            Quantity = 1,
                            UnitPrice = 0m,
                            IsAutoAdded = true
                        });
                    }
                }
                catch { }

                // 3. Active admission / bed charges
                try
                {
                    var admissions = await _services.AdmissionService.GetPatientAdmissionHistoryAsync(patientId.Value);
                    var activeAdmission = admissions.FirstOrDefault(a => a.Status == "Active");
                    if (activeAdmission != null)
                    {
                        var days = (DateTimeOffset.UtcNow - new DateTimeOffset(activeAdmission.AdmissionDate, TimeSpan.Zero)).Days;
                        if (days < 1) days = 1;

                        autoItems.Add(new LineItemEntryViewModel
                        {
                            Description = $"Admission — Ward {activeAdmission.WardName}, Bed {activeAdmission.BedNumber} ({days} day(s))",
                            LineItemType = LineItemType.Other,
                            ReferenceId = activeAdmission.Id.ToString(),
                            Quantity = days,
                            UnitPrice = 0m,
                            IsAutoAdded = true
                        });
                    }
                }
                catch { }

                vm.LineItems = autoItems;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInvoiceViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                PopulateLineItemTypeDropdown();
                return View(vm);
            }

            try
            {
                var request = new CreateInvoiceRequest
                {
                    PatientId = vm.PatientId,
                    AppointmentId = vm.AppointmentId,
                    Notes = vm.Notes,
                    DueDate = vm.DueDate,
                    LineItems = vm.LineItems.Select(li => new AddLineItemRequest
                    {
                        Description = li.Description,
                        LineItemType = li.LineItemType,
                        ReferenceId = li.ReferenceId,
                        Quantity = li.Quantity,
                        UnitPrice = li.UnitPrice
                    }).ToList()
                };

                var invoice = await _services.InvoiceService.CreateInvoiceAsync(request);
                TempData["Success"] = $"Invoice <strong>{invoice.InvoiceNumber}</strong> created successfully.";
                return RedirectToAction(nameof(Details), new { id = invoice.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                PopulateLineItemTypeDropdown();
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLineItem(Guid invoiceId, AddLineItemRequest request)
        {
            try
            {
                await _services.InvoiceService.AddLineItemAsync(invoiceId, request);
                TempData["Success"] = "Line item added.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = invoiceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLineItem(Guid invoiceId, Guid lineItemId)
        {
            try
            {
                await _services.InvoiceService.RemoveLineItemAsync(invoiceId, lineItemId);
                TempData["Success"] = "Line item removed.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = invoiceId });
        }

        public async Task<IActionResult> Issue(Guid id)
        {
            try
            {
                var invoice = await _services.InvoiceService.GetInvoiceByIdAsync(id);

                if (invoice.Status != InvoiceStatus.Draft)
                {
                    TempData["Error"] = "Only Draft invoices can be issued.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(new IssueInvoiceViewModel
                {
                    InvoiceId = id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    CurrentSubTotal = invoice.SubTotal
                });
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Invoice not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Issue(IssueInvoiceViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var request = new IssueInvoiceRequest
                {
                    DiscountAmount = vm.DiscountAmount,
                    DiscountPercent = vm.DiscountPercent,
                    TaxPercent = vm.TaxPercent,
                    Notes = vm.Notes
                };

                var invoice = await _services.InvoiceService.IssueInvoiceAsync(vm.InvoiceId, request);
                TempData["Success"] = $"Invoice <strong>{invoice.InvoiceNumber}</strong> issued. A PDF has been emailed to the patient.";
                return RedirectToAction(nameof(Details), new { id = vm.InvoiceId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                var invoice = await _services.InvoiceService.GetInvoiceByIdAsync(id);

                if (invoice.Status == InvoiceStatus.Cancelled || invoice.Status == InvoiceStatus.Paid)
                {
                    TempData["Error"] = $"Invoice cannot be cancelled because it is already {invoice.Status}.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(new CancelInvoiceViewModel
                {
                    InvoiceId = id,
                    InvoiceNumber = invoice.InvoiceNumber
                });
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Invoice not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CancelInvoiceViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                await _services.InvoiceService.CancelInvoiceAsync(vm.InvoiceId, vm.Reason);
                TempData["Success"] = $"Invoice <strong>{vm.InvoiceNumber}</strong> has been cancelled.";
                return RedirectToAction(nameof(Details), new { id = vm.InvoiceId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        public async Task<IActionResult> DownloadPdf(Guid id)
        {
            try
            {
                var bytes = await _services.InvoiceService.GenerateInvoicePdfAsync(id);
                return File(bytes, "application/pdf", $"invoice-{id}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to generate PDF: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        public async Task<IActionResult> PatientInvoices(int patientId)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientByIdAsync(patientId);
                var invoices = await _services.InvoiceService.GetInvoicesByPatientAsync(patientId);

                ViewBag.PatientId = patientId;
                ViewBag.PatientName = patient.FullName;
                return View(invoices);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction("Index", "Patients");
            }
        }

        private void PopulateLineItemTypeDropdown()
        {
            ViewBag.LineItemTypeList = new SelectList(
                Enum.GetValues<LineItemType>()
                    .Select(v => new { Value = (int)v, Text = v.ToString() }),
                "Value", "Text");
        }
    }
}

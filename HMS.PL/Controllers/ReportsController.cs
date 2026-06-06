using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Parameters;
using HMS.PL.ViewModels.BillingModule;
using Microsoft.AspNetCore.Mvc;

namespace HMS.PL.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IServiceManager _services;

        public ReportsController(IServiceManager services)
        {
            _services = services;
        }

        public async Task<IActionResult> Revenue(
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            int? departmentId = null,
            int? doctorId = null)
        {
            try
            {
                var filters = new ReportFilterParameters
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    DepartmentId = departmentId,
                    DoctorId = doctorId
                };

                var report = await _services.ReportingService.GetRevenueReportAsync(filters);

                var vm = new RevenueReportViewModel
                {
                    Report = report,
                    StartDate = startDate,
                    EndDate = endDate,
                    DepartmentId = departmentId,
                    DoctorId = doctorId
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load report: " + ex.Message;
                return View(new RevenueReportViewModel());
            }
        }

        public async Task<IActionResult> ExportExcel(
            DateOnly? startDate = null,
            DateOnly? endDate = null)
        {
            try
            {
                var filters = new ReportFilterParameters
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                var bytes = await _services.ReportingService.ExportRevenueToExcelAsync(filters);
                var fileName = $"HMS-Revenue-{DateTime.UtcNow:yyyyMMdd}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Export failed: " + ex.Message;
                return RedirectToAction(nameof(Revenue));
            }
        }

        public async Task<IActionResult> Outstanding()
        {
            try
            {
                var invoices = await _services.ReportingService.GetOutstandingInvoicesReportAsync();
                return View(invoices);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load outstanding invoices: " + ex.Message;
                return View(Enumerable.Empty<HMS.BLL.Shared.Dtos.BillingModule.Results.InvoiceSummaryResultDto>());
            }
        }
    }
}

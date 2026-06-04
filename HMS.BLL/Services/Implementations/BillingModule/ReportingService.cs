using AutoMapper;
using ClosedXML.Excel;
using HMS.DAL.Contracts;
using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;
using HMS.DAL.Models.PatientModule;
using HMS.BLL.ServicesAbstraction.Contracts.BillingService;
using HMS.BLL.Services.Specifications.BillingModule;
using HMS.BLL.Services.Specifications.PatientModule;
using HMS.BLL.Shared.Dtos.BillingModule.Results;
using HMS.BLL.Shared.Parameters;

namespace Services.Implementations.BillingModule
{
    public sealed class ReportingService (IUnitOfWork _unitOfWork , IMapper _mapper) : IReportingService
    {
        // ── Revenue Report ────────────────────────────────────────────────────

        public async Task<RevenueReportResultDto> GetRevenueReportAsync(ReportFilterParameters filters)
        {
            var (startDate, endDate) = ResolveDateRange(filters);
            var spec = new InvoiceReportSpecification(startDate, endDate);
            var invoices = (await _unitOfWork.GetRepository<Invoice, Guid>().GetAllAsync(spec)).ToList();

            var totalRevenue = invoices
                .Where(i => i.Status is InvoiceStatus.Paid or InvoiceStatus.PartiallyPaid)
                .Sum(i => i.PaidAmount);

            var totalInvoiced = invoices.Sum(i => i.TotalAmount);

            var totalOutstanding = invoices
                .Where(i => i.Status is not InvoiceStatus.Paid and not InvoiceStatus.Cancelled)
                .Sum(i => i.OutstandingBalance);

            // Revenue by service/line item type
            var byServiceType = invoices
                .SelectMany(i => i.LineItems)
                .GroupBy(li => li.LineItemType)
                .Select(g => new RevenueByGroupResultDto
                {
                    Label = g.Key.ToString(),
                    TotalRevenue = g.Sum(li => li.Total),
                    InvoiceCount = g.Select(li => li.InvoiceId).Distinct().Count()
                })
                .ToList();

            return new RevenueReportResultDto
            {
                Period = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                TotalRevenue = totalRevenue,
                TotalInvoiced = totalInvoiced,
                TotalOutstanding = totalOutstanding,
                ByServiceType = byServiceType,
                ByDoctor = new List<RevenueByGroupResultDto>() // Expandable — requires Appointment join
            };
        }

        // ── Outstanding Invoices ──────────────────────────────────────────────

        public async Task<IEnumerable<InvoiceSummaryResultDto>> GetOutstandingInvoicesReportAsync()
        {
            var statuses = new[] { InvoiceStatus.Issued, InvoiceStatus.PartiallyPaid, InvoiceStatus.Overdue };
            var spec = new InvoicesByStatusSpecification(statuses);
            var invoices = await _unitOfWork.GetRepository<Invoice, Guid>().GetAllAsync(spec);

            // Batch patient names
            var patientIds = invoices.Select(i => i.PatientId).Distinct().ToList();
            var patients = await _unitOfWork.GetRepository<Patient, int>()
                                            .GetAllAsync(new PatientsByIdsSpecification(patientIds));
            var nameMap = patients.ToDictionary(p => p.Id, p => $"{p.FirstName} {p.LastName}");

            return invoices.Select(i =>
            {
                var dto = _mapper.Map<InvoiceSummaryResultDto>(i);
                return dto with { PatientName = nameMap.GetValueOrDefault(i.PatientId, string.Empty) };
            });
        }

        // ── Excel Export ──────────────────────────────────────────────────────

        public async Task<byte[]> ExportRevenueToExcelAsync(ReportFilterParameters filters)
        {
            var (startDate, endDate) = ResolveDateRange(filters);
            var report = await GetRevenueReportAsync(filters);

            var spec = new InvoiceReportSpecification(startDate, endDate);
            var invoices = (await _unitOfWork.GetRepository<Invoice, Guid>().GetAllAsync(spec)).ToList();

            var patientIds = invoices.Select(i => i.PatientId).Distinct().ToList();
            var patients = await _unitOfWork.GetRepository<Patient, int>()
                                            .GetAllAsync(new PatientsByIdsSpecification(patientIds));
            var nameMap = patients.ToDictionary(p => p.Id, p => $"{p.FirstName} {p.LastName}");

            using var workbook = new XLWorkbook();

            // ── Sheet 1: Summary ──────────────────────────────────────────────
            var summaryWs = workbook.Worksheets.Add("Summary");
            var titleCell = summaryWs.Cell(1, 1);
            titleCell.Value = "HMS Revenue Report";
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 14;

            summaryWs.Cell(3, 1).Value = "Period";
            summaryWs.Cell(3, 1).Style.Font.Bold = true;
            summaryWs.Cell(3, 2).Value = report.Period;

            summaryWs.Cell(4, 1).Value = "Total Revenue (Collected)";
            summaryWs.Cell(4, 1).Style.Font.Bold = true;
            summaryWs.Cell(4, 2).Value = report.TotalRevenue;
            summaryWs.Cell(4, 2).Style.NumberFormat.Format = "#,##0.00";

            summaryWs.Cell(5, 1).Value = "Total Invoiced";
            summaryWs.Cell(5, 1).Style.Font.Bold = true;
            summaryWs.Cell(5, 2).Value = report.TotalInvoiced;
            summaryWs.Cell(5, 2).Style.NumberFormat.Format = "#,##0.00";

            summaryWs.Cell(6, 1).Value = "Total Outstanding";
            summaryWs.Cell(6, 1).Style.Font.Bold = true;
            summaryWs.Cell(6, 2).Value = report.TotalOutstanding;
            summaryWs.Cell(6, 2).Style.NumberFormat.Format = "#,##0.00";

            summaryWs.Columns().AdjustToContents();

            // ── Sheet 2: Invoice Details ──────────────────────────────────────
            var detailWs = workbook.Worksheets.Add("Invoice Details");

            string[] headers =
            {
                "Invoice #", "Patient", "Status", "Issued Date",
                "Due Date", "Sub-Total", "Discount", "Tax", "Total", "Paid", "Outstanding"
            };

            for (var col = 0; col < headers.Length; col++)
            {
                var cell = detailWs.Cell(1, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A5F");
                cell.Style.Font.FontColor = XLColor.White;
            }

            var row = 2;
            foreach (var invoice in invoices)
            {
                detailWs.Cell(row, 1).Value = invoice.InvoiceNumber;
                detailWs.Cell(row, 2).Value = nameMap.GetValueOrDefault(invoice.PatientId, "Unknown");
                detailWs.Cell(row, 3).Value = invoice.Status.ToString();
                detailWs.Cell(row, 4).Value = invoice.IssuedAt?.ToString("yyyy-MM-dd") ?? "-";
                detailWs.Cell(row, 5).Value = invoice.DueDate?.ToString("yyyy-MM-dd") ?? "-";

                foreach (var col in new[] { 6, 7, 8, 9, 10, 11 })
                    detailWs.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";

                detailWs.Cell(row, 6).Value = invoice.SubTotal;
                detailWs.Cell(row, 7).Value = invoice.DiscountAmount + (invoice.SubTotal * invoice.DiscountPercent / 100m);
                detailWs.Cell(row, 8).Value = invoice.TaxAmount;
                detailWs.Cell(row, 9).Value = invoice.TotalAmount;
                detailWs.Cell(row, 10).Value = invoice.PaidAmount;
                detailWs.Cell(row, 11).Value = invoice.OutstandingBalance;

                // Colour overdue rows
                if (invoice.Status == InvoiceStatus.Overdue)
                    detailWs.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF3CD");

                row++;
            }

            detailWs.Columns().AdjustToContents();

            // ── Sheet 3: By Service Type ──────────────────────────────────────
            var serviceWs = workbook.Worksheets.Add("By Service Type");
            serviceWs.Cell(1, 1).Value = "Service Type";
            serviceWs.Cell(1, 2).Value = "Total Revenue";
            serviceWs.Cell(1, 3).Value = "Invoice Count";
            serviceWs.Row(1).Style.Font.Bold = true;

            var serviceRow = 2;
            foreach (var item in report.ByServiceType)
            {
                serviceWs.Cell(serviceRow, 1).Value = item.Label;
                serviceWs.Cell(serviceRow, 2).Value = item.TotalRevenue;
                serviceWs.Cell(serviceRow, 2).Style.NumberFormat.Format = "#,##0.00";
                serviceWs.Cell(serviceRow, 3).Value = item.InvoiceCount;
                serviceRow++;
            }

            serviceWs.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static (DateOnly start, DateOnly end) ResolveDateRange(ReportFilterParameters filters)
        {
            var now = DateTime.UtcNow;
            var start = filters.StartDate ?? new DateOnly(now.Year, now.Month, 1);
            var end = filters.EndDate ?? new DateOnly(now.Year, now.Month,
                          DateTime.DaysInMonth(now.Year, now.Month));
            return (start, end);
        }
    }
}

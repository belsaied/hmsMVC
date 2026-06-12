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

namespace HMS.BLL.Services.Implementations.BillingModule
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

            // ── Palette ──────────────────────────────────────────────────────────────
            var navyDark = XLColor.FromHtml("#0F2044");  
            var navyMid = XLColor.FromHtml("#1E3A5F");   
            var accentBlue = XLColor.FromHtml("#0078D4"); 
            var accentTeal = XLColor.FromHtml("#0EA5E9");   
            var gold = XLColor.FromHtml("#F59E0B");   
            var lightGray = XLColor.FromHtml("#F0F4F8");   
            var borderGray = XLColor.FromHtml("#CBD5E1");   
            var white = XLColor.White;
            var textDark = XLColor.FromHtml("#1E293B");

            // Status colours
            var statusColors = new Dictionary<string, XLColor>(StringComparer.OrdinalIgnoreCase)
    {
        { "Paid",          XLColor.FromHtml("#D1FAE5") },
        { "PartiallyPaid", XLColor.FromHtml("#DBEAFE") },
        { "Issued",        XLColor.FromHtml("#FEF9C3") },
        { "Overdue",       XLColor.FromHtml("#FEE2E2") },
        { "Draft",         XLColor.FromHtml("#F1F5F9") },
        { "Cancelled",     XLColor.FromHtml("#F1F5F9") },
    };

            using var workbook = new XLWorkbook();
            workbook.Style.Font.FontName = "Arial";

            // ════════════════════════════════════════════════════════════════════════
            //  SHEET 1 — SUMMARY DASHBOARD
            // ════════════════════════════════════════════════════════════════════════
            var ws = workbook.Worksheets.Add("Summary");
            ws.ShowGridLines = false;
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.FitToPages(1, 1);

            // ── Cover banner (rows 1-4) ──────────────────────────────────────────────
            ws.Range("A1:L4").Merge();
            var banner = ws.Cell("A1");
            banner.Value = "HEALING — HOSPITAL MANAGEMENT SYSTEM";
            banner.Style
                  .Font.SetFontName("Arial")
                  .Font.SetFontSize(22)
                  .Font.SetBold(true)
                  .Font.SetFontColor(white)
                  .Fill.SetBackgroundColor(navyDark)
                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                  .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Row(1).Height = 20;
            ws.Row(2).Height = 20;
            ws.Row(3).Height = 20;
            ws.Row(4).Height = 20;

            // Sub-title row 5
            ws.Range("A5:L5").Merge();
            var sub = ws.Cell("A5");
            sub.Value = $"Revenue Report  ·  Period: {report.Period}  ·  Generated: {DateTime.Now:dd MMM yyyy HH:mm}";
            sub.Style
               .Font.SetFontName("Arial")
               .Font.SetFontSize(10)
               .Font.SetFontColor(white)
               .Fill.SetBackgroundColor(accentBlue)
               .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
               .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Row(5).Height = 18;

            ws.Row(6).Height = 10; // spacer

            // ── KPI Cards (row 7-11, three cards side-by-side) ───────────────────────
            void DrawKpiCard(IXLWorksheet sheet, string col1, string col2, string label, decimal value, XLColor accent)
            {
                // Header strip
                sheet.Range($"{col1}7:{col2}7").Merge();
                var hdr = sheet.Cell($"{col1}7");
                hdr.Value = label.ToUpperInvariant();
                hdr.Style
                   .Font.SetFontName("Arial")
                   .Font.SetFontSize(8)
                   .Font.SetBold(true)
                   .Font.SetFontColor(white)
                   .Fill.SetBackgroundColor(accent)
                   .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                   .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                sheet.Row(7).Height = 16;

                // Value
                sheet.Range($"{col1}8:{col2}10").Merge();
                var val = sheet.Cell($"{col1}8");
                val.Value = value;
                val.Style
                   .NumberFormat.SetFormat("\"EGP \"#,##0.00")
                   .Font.SetFontName("Arial")
                   .Font.SetFontSize(18)
                   .Font.SetBold(true)
                   .Font.SetFontColor(textDark)
                   .Fill.SetBackgroundColor(white)
                   .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                   .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                   .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                   .Border.SetOutsideBorderColor(borderGray);
                sheet.Row(8).Height = 20;
                sheet.Row(9).Height = 20;
                sheet.Row(10).Height = 20;

                // Bottom label
                sheet.Range($"{col1}11:{col2}11").Merge();
                var lbl = sheet.Cell($"{col1}11");
                lbl.Value = label;
                lbl.Style
                   .Font.SetFontName("Arial")
                   .Font.SetFontSize(8)
                   .Font.SetItalic(true)
                   .Font.SetFontColor(XLColor.FromHtml("#64748B"))
                   .Fill.SetBackgroundColor(lightGray)
                   .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                   .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                   .Border.SetOutsideBorderColor(borderGray);
                sheet.Row(11).Height = 14;
            }

            DrawKpiCard(ws, "A", "D", "Total Revenue Collected", report.TotalRevenue, accentTeal);
            DrawKpiCard(ws, "E", "H", "Total Invoiced", report.TotalInvoiced, accentBlue);
            DrawKpiCard(ws, "I", "L", "Total Outstanding", report.TotalOutstanding, gold);

            ws.Row(12).Height = 14; // spacer

            // ── Revenue by Service Type table (row 13 onwards) ───────────────────────
            ws.Range("A13:L13").Merge();
            var secHdr = ws.Cell("A13");
            secHdr.Value = "REVENUE BY SERVICE TYPE";
            secHdr.Style
                  .Font.SetFontName("Arial")
                  .Font.SetFontSize(10)
                  .Font.SetBold(true)
                  .Font.SetFontColor(white)
                  .Fill.SetBackgroundColor(navyMid)
                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                  .Alignment.SetIndent(2);
            ws.Row(13).Height = 18;

            // Table headers row 14
            string[] svcHeaders = { "Service Type", "Total Revenue (EGP)", "Invoice Count", "% of Total" };
            int[] svcCols = { 1, 5, 9, 11 };   // column indices (1-based)
            for (int i = 0; i < svcHeaders.Length; i++)
            {
                var c = ws.Cell(14, svcCols[i]);
                c.Value = svcHeaders[i];
                c.Style
                 .Font.SetFontName("Arial")
                 .Font.SetFontSize(9)
                 .Font.SetBold(true)
                 .Font.SetFontColor(white)
                 .Fill.SetBackgroundColor(navyMid)
                 .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                 .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                 .Border.SetOutsideBorderColor(borderGray);
            }
            ws.Row(14).Height = 16;

            int svcRow = 15;
            bool alt = false;
            decimal grandTotal = report.ByServiceType.Sum(x => x.TotalRevenue);
            foreach (var item in report.ByServiceType.OrderByDescending(x => x.TotalRevenue))
            {
                var fill = alt ? lightGray : white;
                alt = !alt;

                void SetSvcCell(int col, object val, string? fmt = null)
                {
                    var cell = ws.Cell(svcRow, col);
                    if (val is decimal d) cell.Value = d;
                    else if (val is int iv) cell.Value = iv;
                    else cell.Value = val?.ToString() ?? "";
                    cell.Style
                        .Font.SetFontName("Arial")
                        .Font.SetFontSize(9)
                        .Fill.SetBackgroundColor(fill)
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                        .Border.SetOutsideBorderColor(borderGray);
                    if (fmt != null) cell.Style.NumberFormat.SetFormat(fmt);
                }

                SetSvcCell(1, item.Label);
                SetSvcCell(5, item.TotalRevenue, "\"EGP \"#,##0.00");
                SetSvcCell(9, item.InvoiceCount, "#,##0");
                double pct = grandTotal > 0 ? (double)(item.TotalRevenue / grandTotal) : 0;
                var pctCell = ws.Cell(svcRow, 11);
                pctCell.Value = pct;
                pctCell.Style
                       .Font.SetFontName("Arial")
                       .Font.SetFontSize(9)
                       .Fill.SetBackgroundColor(fill)
                       .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                       .NumberFormat.SetFormat("0.0%")
                       .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                       .Border.SetOutsideBorderColor(borderGray);
                ws.Row(svcRow).Height = 15;
                svcRow++;
            }

            // Totals row
            var totRow = ws.Row(svcRow);
            totRow.Height = 16;
            void TotCell(int col, object val, string? fmt = null)
            {
                var c = ws.Cell(svcRow, col);
                if (val is decimal dv) c.Value = dv;
                else if (val is int iv) c.Value = iv;
                else c.Value = val?.ToString() ?? "";
                c.Style
                 .Font.SetFontName("Arial")
                 .Font.SetFontSize(9)
                 .Font.SetBold(true)
                 .Fill.SetBackgroundColor(XLColor.FromHtml("#E2EBF7"))
                 .Border.SetOutsideBorder(XLBorderStyleValues.Medium)
                 .Border.SetOutsideBorderColor(navyMid);
                if (fmt != null) c.Style.NumberFormat.SetFormat(fmt);
            }
            TotCell(1, "TOTAL");
            TotCell(5, grandTotal, "\"EGP \"#,##0.00");
            TotCell(9, report.ByServiceType.Sum(x => x.InvoiceCount), "#,##0");
            TotCell(11, 1.0, "0.0%");

            // ── Column widths ─────────────────────────────────────────────────────────
            for (int c = 1; c <= 12; c++) ws.Column(c).Width = 12;
            ws.Column(1).Width = 22;

            // ════════════════════════════════════════════════════════════════════════
            //  SHEET 2 — INVOICE DETAILS
            // ════════════════════════════════════════════════════════════════════════
            var dws = workbook.Worksheets.Add("Invoice Details");
            dws.ShowGridLines = false;
            dws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            dws.PageSetup.FitToPages(1, 0);

            // Title banner
            dws.Range("A1:K2").Merge();
            var dtitle = dws.Cell("A1");
            dtitle.Value = "Invoice Details";
            dtitle.Style
                  .Font.SetFontName("Arial")
                  .Font.SetFontSize(16)
                  .Font.SetBold(true)
                  .Font.SetFontColor(white)
                  .Fill.SetBackgroundColor(navyDark)
                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                  .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            dws.Row(1).Height = 22;
            dws.Row(2).Height = 16;

            dws.Range("A3:K3").Merge();
            var dperiod = dws.Cell("A3");
            dperiod.Value = $"Period: {report.Period}  |  Total invoices: {invoices.Count}";
            dperiod.Style
                   .Font.SetFontName("Arial")
                   .Font.SetFontSize(9)
                   .Font.SetFontColor(white)
                   .Fill.SetBackgroundColor(accentBlue)
                   .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            dws.Row(3).Height = 14;
            dws.Row(4).Height = 8; // spacer

            // Column header definitions
            var detailHeaders = new (string Label, int Width, XLAlignmentHorizontalValues Align)[]
            {
        ("Invoice #",     18, XLAlignmentHorizontalValues.Left),
        ("Patient",       24, XLAlignmentHorizontalValues.Left),
        ("Status",        14, XLAlignmentHorizontalValues.Center),
        ("Issued Date",   14, XLAlignmentHorizontalValues.Center),
        ("Due Date",      14, XLAlignmentHorizontalValues.Center),
        ("Sub-Total",     15, XLAlignmentHorizontalValues.Right),
        ("Discount",      13, XLAlignmentHorizontalValues.Right),
        ("Tax",           12, XLAlignmentHorizontalValues.Right),
        ("Total (EGP)",   15, XLAlignmentHorizontalValues.Right),
        ("Paid (EGP)",    15, XLAlignmentHorizontalValues.Right),
        ("Outstanding",   15, XLAlignmentHorizontalValues.Right),
            };

            for (int i = 0; i < detailHeaders.Length; i++)
            {
                var (lbl, w, align) = detailHeaders[i];
                var hdrCell = dws.Cell(5, i + 1);
                hdrCell.Value = lbl;
                hdrCell.Style
                       .Font.SetFontName("Arial")
                       .Font.SetFontSize(9)
                       .Font.SetBold(true)
                       .Font.SetFontColor(white)
                       .Fill.SetBackgroundColor(navyMid)
                       .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                       .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                       .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                       .Border.SetOutsideBorderColor(white);
                dws.Column(i + 1).Width = w;
            }
            dws.Row(5).Height = 18;

            // Data rows
            int dRow = 6;
            bool dAlt = false;
            foreach (var inv in invoices.OrderByDescending(i => i.IssuedAt ?? DateTime.MinValue))
            {
                var rowFill = dAlt ? lightGray : white;
                var statusKey = inv.Status.ToString();
                var statusFill = statusColors.TryGetValue(statusKey, out var sc) ? sc : lightGray;
                dAlt = !dAlt;

                void DCell(int col, object val, string? fmt = null,
                           XLAlignmentHorizontalValues align = XLAlignmentHorizontalValues.Left,
                           XLColor? customFill = null)
                {
                    var c = dws.Cell(dRow, col);
                    switch (val)
                    {
                        case decimal d: c.Value = d; break;
                        case int iv: c.Value = iv; break;
                        default: c.Value = val?.ToString() ?? "—"; break;
                    }
                    c.Style
                     .Font.SetFontName("Arial")
                     .Font.SetFontSize(8)
                     .Fill.SetBackgroundColor(customFill ?? rowFill)
                     .Alignment.SetHorizontal(align)
                     .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                     .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                     .Border.SetOutsideBorderColor(borderGray);
                    if (fmt != null) c.Style.NumberFormat.SetFormat(fmt);
                }

                var discountTotal = inv.DiscountAmount +
                                    (inv.SubTotal * inv.DiscountPercent / 100m);

                DCell(1, inv.InvoiceNumber, align: XLAlignmentHorizontalValues.Left);
                DCell(2, nameMap.GetValueOrDefault(inv.PatientId, "Unknown"),
                      align: XLAlignmentHorizontalValues.Left);
                DCell(3, inv.Status.ToString(),
                      align: XLAlignmentHorizontalValues.Center,
                      customFill: statusFill);
                DCell(4, inv.IssuedAt?.ToString("dd MMM yyyy") ?? "—",
                      align: XLAlignmentHorizontalValues.Center);
                DCell(5, inv.DueDate?.ToString("dd MMM yyyy") ?? "—",
                      align: XLAlignmentHorizontalValues.Center);
                DCell(6, inv.SubTotal, "#,##0.00", XLAlignmentHorizontalValues.Right);
                DCell(7, discountTotal, "#,##0.00", XLAlignmentHorizontalValues.Right);
                DCell(8, inv.TaxAmount, "#,##0.00", XLAlignmentHorizontalValues.Right);
                DCell(9, inv.TotalAmount, "#,##0.00", XLAlignmentHorizontalValues.Right);
                DCell(10, inv.PaidAmount, "#,##0.00", XLAlignmentHorizontalValues.Right);

                // Outstanding cell — bold red if overdue
                var outCell = dws.Cell(dRow, 11);
                outCell.Value = inv.OutstandingBalance;
                outCell.Style
                       .Font.SetFontName("Arial")
                       .Font.SetFontSize(8)
                       .Font.SetBold(inv.Status == InvoiceStatus.Overdue)
                       .Font.SetFontColor(inv.Status == InvoiceStatus.Overdue
                           ? XLColor.FromHtml("#DC2626") : textDark)
                       .Fill.SetBackgroundColor(inv.Status == InvoiceStatus.Overdue
                           ? XLColor.FromHtml("#FEE2E2") : rowFill)
                       .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                       .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                       .NumberFormat.SetFormat("#,##0.00")
                       .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                       .Border.SetOutsideBorderColor(borderGray);

                dws.Row(dRow).Height = 15;
                dRow++;
            }

            // Totals row
            if (invoices.Any())
            {
                void DTot(int col, object val, string? fmt = null,
                          XLAlignmentHorizontalValues align = XLAlignmentHorizontalValues.Right)
                {
                    var c = dws.Cell(dRow, col);
                    if (val is decimal dv) c.Value = dv;
                    else c.Value = val?.ToString() ?? "";
                    c.Style
                     .Font.SetFontName("Arial")
                     .Font.SetFontSize(9)
                     .Font.SetBold(true)
                     .Fill.SetBackgroundColor(XLColor.FromHtml("#E2EBF7"))
                     .Alignment.SetHorizontal(align)
                     .Border.SetOutsideBorder(XLBorderStyleValues.Medium)
                     .Border.SetOutsideBorderColor(navyMid);
                    if (fmt != null) c.Style.NumberFormat.SetFormat(fmt);
                }
                DTot(1, "TOTALS", align: XLAlignmentHorizontalValues.Left);
                DTot(2, "", align: XLAlignmentHorizontalValues.Left);
                DTot(3, "");
                DTot(4, "");
                DTot(5, "");
                DTot(6, invoices.Sum(i => i.SubTotal), "#,##0.00");
                DTot(7, invoices.Sum(i => i.DiscountAmount + (i.SubTotal * i.DiscountPercent / 100m)), "#,##0.00");
                DTot(8, invoices.Sum(i => i.TaxAmount), "#,##0.00");
                DTot(9, invoices.Sum(i => i.TotalAmount), "#,##0.00");
                DTot(10, invoices.Sum(i => i.PaidAmount), "#,##0.00");
                DTot(11, invoices.Sum(i => i.OutstandingBalance), "#,##0.00");
                dws.Row(dRow).Height = 16;
            }

            // Freeze header row
            dws.SheetView.Freeze(5, 0);

            // ════════════════════════════════════════════════════════════════════════
            //  SHEET 3 — SERVICE BREAKDOWN
            // ════════════════════════════════════════════════════════════════════════
            var sws = workbook.Worksheets.Add("By Service Type");
            sws.ShowGridLines = false;
            sws.PageSetup.PageOrientation = XLPageOrientation.Portrait;

            // Banner
            sws.Range("A1:E2").Merge();
            var stitle = sws.Cell("A1");
            stitle.Value = "Revenue by Service Type";
            stitle.Style
                  .Font.SetFontName("Arial")
                  .Font.SetFontSize(16)
                  .Font.SetBold(true)
                  .Font.SetFontColor(white)
                  .Fill.SetBackgroundColor(navyDark)
                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                  .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            sws.Row(1).Height = 22;
            sws.Row(2).Height = 16;

            sws.Range("A3:E3").Merge();
            var speriod = sws.Cell("A3");
            speriod.Value = $"Period: {report.Period}";
            speriod.Style
                   .Font.SetFontName("Arial").Font.SetFontSize(9)
                   .Font.SetFontColor(white)
                   .Fill.SetBackgroundColor(accentBlue)
                   .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            sws.Row(3).Height = 14;
            sws.Row(4).Height = 8;

            // Headers
            var sHeaders = new[] { "Service Type", "Total Revenue (EGP)", "Invoice Count", "% of Revenue", "Rank" };
            for (int i = 0; i < sHeaders.Length; i++)
            {
                var c = sws.Cell(5, i + 1);
                c.Value = sHeaders[i];
                c.Style
                 .Font.SetFontName("Arial").Font.SetFontSize(9).Font.SetBold(true)
                 .Font.SetFontColor(white)
                 .Fill.SetBackgroundColor(navyMid)
                 .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                 .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                 .Border.SetOutsideBorderColor(white);
            }
            sws.Column(1).Width = 22;
            sws.Column(2).Width = 20;
            sws.Column(3).Width = 16;
            sws.Column(4).Width = 16;
            sws.Column(5).Width = 10;
            sws.Row(5).Height = 18;

            var ranked = report.ByServiceType
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();
            decimal svcGrand = ranked.Sum(x => x.TotalRevenue);

            int sRow = 6;
            bool sAlt = false;
            int rank = 1;
            foreach (var item in ranked)
            {
                var fill = sAlt ? lightGray : white;
                sAlt = !sAlt;

                void SCell(int col, object val, string? fmt = null,
                           XLAlignmentHorizontalValues align = XLAlignmentHorizontalValues.Center)
                {
                    var c = sws.Cell(sRow, col);
                    if (val is decimal dv) c.Value = dv;
                    else if (val is int iv) c.Value = iv;
                    else c.Value = val?.ToString() ?? "";
                    c.Style
                     .Font.SetFontName("Arial").Font.SetFontSize(9)
                     .Fill.SetBackgroundColor(fill)
                     .Alignment.SetHorizontal(align)
                     .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                     .Border.SetOutsideBorderColor(borderGray);
                    if (fmt != null) c.Style.NumberFormat.SetFormat(fmt);
                }

                double pct = svcGrand > 0 ? (double)(item.TotalRevenue / svcGrand) : 0;
                SCell(1, item.Label, align: XLAlignmentHorizontalValues.Left);
                SCell(2, item.TotalRevenue, "#,##0.00");
                SCell(3, item.InvoiceCount, "#,##0");
                var pctCell2 = sws.Cell(sRow, 4);
                pctCell2.Value = pct;
                pctCell2.Style.Font.SetFontName("Arial").Font.SetFontSize(9)
                       .Fill.SetBackgroundColor(fill)
                       .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                       .NumberFormat.SetFormat("0.0%")
                       .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                       .Border.SetOutsideBorderColor(borderGray);
                SCell(5, rank);
                sws.Row(sRow).Height = 15;
                sRow++;
                rank++;
            }

            // Totals row
            void STot(int col, object val, string? fmt = null,
                      XLAlignmentHorizontalValues align = XLAlignmentHorizontalValues.Center)
            {
                var c = sws.Cell(sRow, col);
                if (val is decimal dv) c.Value = dv;
                else if (val is int iv) c.Value = iv;
                else c.Value = val?.ToString() ?? "";
                c.Style
                 .Font.SetFontName("Arial").Font.SetFontSize(9).Font.SetBold(true)
                 .Fill.SetBackgroundColor(XLColor.FromHtml("#E2EBF7"))
                 .Alignment.SetHorizontal(align)
                 .Border.SetOutsideBorder(XLBorderStyleValues.Medium)
                 .Border.SetOutsideBorderColor(navyMid);
                if (fmt != null) c.Style.NumberFormat.SetFormat(fmt);
            }
            STot(1, "TOTAL", align: XLAlignmentHorizontalValues.Left);
            STot(2, svcGrand, "#,##0.00");
            STot(3, ranked.Sum(x => x.InvoiceCount), "#,##0");
            var totPctCell = sws.Cell(sRow, 4);
            totPctCell.Value = 1.0;
            totPctCell.Style.Font.SetFontName("Arial").Font.SetFontSize(9).Font.SetBold(true)
                      .Fill.SetBackgroundColor(XLColor.FromHtml("#E2EBF7"))
                      .NumberFormat.SetFormat("0.0%")
                      .Border.SetOutsideBorder(XLBorderStyleValues.Medium)
                      .Border.SetOutsideBorderColor(navyMid);
            STot(5, "");
            sws.Row(sRow).Height = 16;

            // ── Set active sheet ──────────────────────────────────────────────────────
            workbook.Worksheet("Summary").SetTabActive();

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

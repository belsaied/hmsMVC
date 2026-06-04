using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HMS.BLL.ServicesAbstraction.Contracts.BillingService;

namespace HMS.BLL.Services.Implementations.BillingModule
{
    public sealed class InvoicePdfGenerator : IInvoicePdfGenerator
    {
        public byte[] Generate(Invoice invoice, string patientName, string patientEmail)
        {
            var document = new InvoicePdfDocument(invoice, patientName, patientEmail);
            return document.GeneratePdf();
        }
    }

    // ── Document ──────────────────────────────────────────────────────────────

    internal sealed class InvoicePdfDocument(
        Invoice invoice,
        string patientName,
        string patientEmail) : IDocument
    {
        // Colour constants — plain strings, no type ambiguity
        private const string PrimaryColor = "#1E3A5F";
        private const string AccentColor = "#0078D4";
        private const string LightGray = "#F5F5F5";
        private const string BorderGray = "#D0D0D0";

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        // ── Header ────────────────────────────────────────────────────────────

        private void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(inner =>
                    {
                        inner.Item().Text("HMS — Hospital Management System")
                            .FontSize(18).Bold().FontColor(PrimaryColor);

                        inner.Item().Text("Medical Invoice")
                            .FontSize(11).FontColor(AccentColor);
                    });

                    row.ConstantItem(180).Column(inner =>
                    {
                        inner.Item().AlignRight().Text(invoice.InvoiceNumber)
                            .FontSize(13).Bold().FontColor(PrimaryColor);

                        inner.Item().AlignRight()
                            .Text($"Issued: {invoice.IssuedAt?.ToString("dd MMM yyyy") ?? "Draft"}")
                            .FontSize(9).FontColor(Colors.Grey.Medium);

                        if (invoice.DueDate.HasValue)
                            inner.Item().AlignRight()
                                .Text($"Due: {invoice.DueDate:dd MMM yyyy}")
                                .FontSize(9).FontColor(Colors.Red.Medium);
                    });
                });

                col.Item().PaddingVertical(4).LineHorizontal(1).LineColor(BorderGray);
            });
        }

        // ── Content ───────────────────────────────────────────────────────────

        private void ComposeContent(IContainer container)
        {
            container.Column(col =>
            {
                col.Spacing(12);
                col.Item().Element(ComposePatientInfo);
                col.Item().Element(ComposeLineItemsTable);
                col.Item().Element(ComposeTotals);

                if (invoice.PaidAmount > 0)
                    col.Item().Element(ComposePaymentSummary);

                if (!string.IsNullOrWhiteSpace(invoice.Notes))
                {
                    col.Item().Background(LightGray).Padding(8).Column(n =>
                    {
                        n.Item().Text("Notes").Bold().FontSize(9);
                        n.Item().Text(invoice.Notes!).FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                }
            });
        }

        private void ComposePatientInfo(IContainer container)
        {
            container.Background(LightGray).Padding(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Bill To").Bold().FontSize(9).FontColor(AccentColor);
                    col.Item().Text(patientName).Bold().FontSize(11);

                    if (!string.IsNullOrWhiteSpace(patientEmail))
                        col.Item().Text(patientEmail).FontSize(9).FontColor(Colors.Grey.Darken1);

                    col.Item().Text($"Patient ID: {invoice.PatientId}")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Invoice Status").Bold().FontSize(9).FontColor(AccentColor);

                    string statusColor = invoice.Status == InvoiceStatus.Paid
                        ? Colors.Green.Darken2
                        : PrimaryColor;

                    col.Item().Text(invoice.Status.ToString())
                        .Bold().FontSize(11).FontColor(statusColor);
                });
            });
        }

        private void ComposeLineItemsTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(4);   // Description
                    cols.RelativeColumn(2);   // Type
                    cols.ConstantColumn(50);  // Qty
                    cols.ConstantColumn(80);  // Unit Price
                    cols.ConstantColumn(80);  // Total
                });

                table.Header(header =>
                {
                    foreach (var label in new[] { "Description", "Type", "Qty", "Unit Price", "Total" })
                    {
                        header.Cell()
                            .Background(PrimaryColor)
                            .Padding(5)
                            .Text(label)
                            .Bold()
                            .FontSize(9)
                            .FontColor(Colors.White);
                    }
                });

                var isOdd = false;
                foreach (var item in invoice.LineItems)
                {
                    string bg = isOdd ? Colors.White : LightGray;
                    isOdd = !isOdd;

                    table.Cell().Background(bg).Padding(5).Text(item.Description).FontSize(9);
                    table.Cell().Background(bg).Padding(5).Text(item.LineItemType.ToString()).FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignCenter().Text(item.Quantity.ToString()).FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text($"{item.UnitPrice:N2}").FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text($"{item.Total:N2}").FontSize(9).Bold();
                }
            });
        }

        private void ComposeTotals(IContainer container)
        {
            container.AlignRight().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(160);
                    cols.ConstantColumn(100);
                });

                // Helper: add a plain row (no bold, no colour override)
                void Row(string label, string value)
                {
                    table.Cell().Padding(3).Text(label).FontSize(9);
                    table.Cell().Padding(3).AlignRight().Text(value).FontSize(9);
                }

                // Helper: add a bold coloured row
                void BoldRow(string label, string value, string color)
                {
                    table.Cell().Padding(3).Text(label).Bold().FontSize(9).FontColor(color);
                    table.Cell().Padding(3).AlignRight().Text(value).Bold().FontSize(9).FontColor(color);
                }

                Row("Sub-Total", $"{invoice.SubTotal:N2}");

                var totalDiscount = invoice.DiscountAmount +
                                    (invoice.SubTotal * invoice.DiscountPercent / 100m);
                if (totalDiscount > 0)
                    BoldRow("Discount", $"- {totalDiscount:N2}", Colors.Green.Darken2);

                if (invoice.TaxPercent > 0)
                    Row($"Tax ({invoice.TaxPercent:N1}%)", $"{invoice.TaxAmount:N2}");

                table.Cell().ColumnSpan(2).LineHorizontal(1).LineColor(BorderGray);

                BoldRow("Total", $"{invoice.TotalAmount:N2}", PrimaryColor);
            });
        }

        private void ComposePaymentSummary(IContainer container)
        {
            container.Background(LightGray).Padding(8).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Payment Summary").Bold().FontSize(9).FontColor(AccentColor);

                    foreach (var p in invoice.Payments
                                 .Where(p => p.Status == PaymentStatus.Succeeded))
                    {
                        col.Item()
                           .Text($"{p.PaidAt?.ToString("dd MMM yyyy")} — {p.PaymentMethod} — {p.Amount:N2}")
                           .FontSize(9);
                    }
                });

                row.ConstantItem(150).Column(col =>
                {
                    col.Item().AlignRight()
                        .Text($"Paid: {invoice.PaidAmount:N2}")
                        .FontSize(9).FontColor(Colors.Green.Darken2);

                    // ← Fix CS0172: resolve colour to a string variable first
                    var balanceColor = invoice.OutstandingBalance > 0
                        ? Colors.Red.Medium
                        : Colors.Green.Darken2;

                    col.Item().AlignRight()
                        .Text($"Outstanding: {invoice.OutstandingBalance:N2}")
                        .Bold().FontSize(10).FontColor(balanceColor);
                });
            });
        }

        // ── Footer ────────────────────────────────────────────────────────────

        private void ComposeFooter(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(BorderGray);
                col.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem()
                        .Text("Thank you for choosing HMS. Please retain this invoice for your records.")
                        .FontSize(8).FontColor(Colors.Grey.Medium);

                    row.ConstantItem(80).AlignRight().Text(text =>
                    {
                        text.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });
        }
    }
}

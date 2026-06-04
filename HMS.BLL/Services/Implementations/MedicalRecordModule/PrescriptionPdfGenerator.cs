using HMS.DAL.Models.MedicalRecordModule;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HMS.BLL.ServicesAbstraction.Contracts;

namespace HMS.BLL.Services.Implementations.MedicalRecordModule
{
    public sealed class PrescriptionPdfGenerator : IPrescriptionPdfGenerator
    {
        public byte[] Generate(
            Prescription prescription,
            string patientName,
            string doctorName,
            string doctorSpecialization)
        {
            var document = new PrescriptionPdfDocument(
                prescription, patientName, doctorName, doctorSpecialization);
            return document.GeneratePdf();
        }
    }

    internal sealed class PrescriptionPdfDocument(
        Prescription prescription,
        string patientName,
        string doctorName,
        string doctorSpecialization) : IDocument
    {
        private const string PrimaryColor = "#1E3A5F";
        private const string AccentColor = "#0078D4";
        private const string LightGray = "#F5F5F5";
        private const string BorderGray = "#D0D0D0";
        private const string YellowNote = "#FFF9C4";

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
                        inner.Item().Text("Prescription Details")
                            .FontSize(11).FontColor(AccentColor);
                    });

                    row.ConstantItem(200).Column(inner =>
                    {
                        inner.Item().AlignRight()
                            .Text($"Prescription #RX-{prescription.Id:D8}")
                            .FontSize(11).Bold().FontColor(PrimaryColor);
                        inner.Item().AlignRight()
                            .Text($"Date of Issue: {prescription.PrescribedAt:dd MMM yyyy}")
                            .FontSize(9).FontColor(Colors.Grey.Medium);
                        inner.Item().AlignRight()
                            .Text($"Expires: {prescription.ExpiresAt:dd MMM yyyy}")
                            .FontSize(9).FontColor(Colors.Red.Medium);
                    });
                });

                col.Item().PaddingVertical(4).LineHorizontal(1).LineColor(BorderGray);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(col =>
            {
                col.Spacing(14);

                // Doctor Info Card
                col.Item().Element(ComposeDoctorInfo);

                // Meta row: Date of Issue + Prescription ID
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(inner =>
                    {
                        inner.Item().Text("DATE OF ISSUE")
                            .FontSize(8).FontColor(Colors.Grey.Medium);
                        inner.Item().Text(prescription.PrescribedAt.ToString("MMM dd, yyyy"))
                            .FontSize(10).Bold();
                    });
                    row.RelativeItem().Column(inner =>
                    {
                        inner.Item().Text("PRESCRIPTION ID")
                            .FontSize(8).FontColor(Colors.Grey.Medium);
                        inner.Item().Text($"#RX-{prescription.Id:D8}")
                            .FontSize(10).Bold();
                    });
                });

                // Medications Section
                col.Item().Element(ComposeMedicationsSection);

                // Doctor's Notes Section
                if (!string.IsNullOrWhiteSpace(prescription.Instructions))
                    col.Item().Element(ComposeNotesSection);

                // Doctor Signature
                col.Item().Element(ComposeSignature);
            });
        }

        private void ComposeDoctorInfo(IContainer container)
        {
            container.Background(LightGray).Padding(12).Row(row =>
            {
                // Doctor avatar placeholder circle
                row.ConstantItem(52).Height(52).Background(AccentColor)
                    .AlignCenter().AlignMiddle()
                    .Text("Dr")
                    .FontSize(14).Bold().FontColor(Colors.White);

                row.ConstantItem(12); // spacer

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Dr. {doctorName}")
                        .FontSize(14).Bold().FontColor(PrimaryColor);
                    col.Item().Text(doctorSpecialization)
                        .FontSize(10).FontColor(AccentColor);
                });

                row.ConstantItem(80).AlignRight().AlignMiddle()
                    .Background(Colors.Green.Lighten3).Padding(4)
                    .Text(prescription.Status.ToString().ToUpperInvariant())
                    .FontSize(9).Bold().FontColor(Colors.Green.Darken2);
            });
        }

        private void ComposeMedicationsSection(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Medications")
                    .FontSize(13).Bold().FontColor(PrimaryColor);

                col.Item().PaddingTop(6).Border(1).BorderColor(BorderGray)
                    .Padding(12).Column(medCol =>
                    {
                        medCol.Item().Row(row =>
                        {
                            // Pill icon placeholder
                            row.ConstantItem(32).Height(32)
                                .Background(AccentColor)
                                .AlignCenter().AlignMiddle()
                                .Text("Rx").FontSize(9).Bold().FontColor(Colors.White);

                            row.ConstantItem(10);

                            row.RelativeItem().Column(inner =>
                            {
                                inner.Item().Text(prescription.MedicationName)
                                    .FontSize(12).Bold();
                                inner.Item().Text($"{prescription.Dosage} • {GetFormType(prescription.MedicationName)}")
                                    .FontSize(9).FontColor(Colors.Grey.Medium);
                                inner.Item().PaddingTop(4).Row(freqRow =>
                                {
                                    freqRow.AutoItem()
                                        .Text("⏰ ").FontSize(9).FontColor(AccentColor);
                                    freqRow.RelativeItem()
                                        .Text(prescription.Frequency)
                                        .FontSize(9).FontColor(AccentColor);
                                });
                            });

                            row.ConstantItem(60).AlignRight().AlignMiddle()
                                .Text($"{prescription.DurationDays} Days")
                                .FontSize(9).Bold();
                        });
                    });
            });
        }

        private void ComposeNotesSection(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Doctor's Notes")
                    .FontSize(13).Bold().FontColor(PrimaryColor);

                col.Item().PaddingTop(6)
                    .Background(YellowNote).Padding(12).Column(noteCol =>
                    {
                        noteCol.Item()
                            .Text($"\"{prescription.Instructions}\"")
                            .FontSize(10).Italic().FontColor(Colors.Grey.Darken2);
                    });
            });
        }

        private void ComposeSignature(IContainer container)
        {
            container.AlignRight().Column(col =>
            {
                col.Item().AlignRight()
                    .Text($"Dr. {doctorName}, M.D.")
                    .FontSize(12).Bold().FontColor(PrimaryColor);
                col.Item().AlignRight()
                    .Text(doctorSpecialization)
                    .FontSize(9).FontColor(Colors.Grey.Medium);
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(BorderGray);
                col.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem()
                        .Text("This prescription was generated by HMS. Please retain for your records.")
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

        private static string GetFormType(string medicationName)
        {
            var name = medicationName.ToLowerInvariant();
            if (name.Contains("syrup") || name.Contains("solution")) return "Syrup";
            if (name.Contains("injection") || name.Contains("inj")) return "Injection";
            if (name.Contains("cream") || name.Contains("gel")) return "Topical";
            if (name.Contains("drops")) return "Drops";
            return "Tablet";
        }
    }
}

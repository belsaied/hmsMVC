using HMS.DAL.Models.Enums.BillingEnums;

namespace HMS.BLL.Shared.Dtos.BillingModule.Results
{
    public record InvoiceSummaryResultDto
    {
        public Guid Id { get; init; }
        public string InvoiceNumber { get; init; } = string.Empty;
        public string PatientName { get; init; } = string.Empty;
        public InvoiceStatus Status { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal OutstandingBalance { get; init; }
        public DateOnly? DueDate { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}

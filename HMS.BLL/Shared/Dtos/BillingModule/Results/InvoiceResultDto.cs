using HMS.DAL.Models.Enums.BillingEnums;
using HMS.BLL.Shared.Dtos.BillingModule.Results;
using HMS.BLL.Shared.Dtos.PatientModule.PatientDtos;

namespace HMS.BLL.Shared.Dtos.BillingModule.Results
{
    public record InvoiceResultDto
    {
        public Guid Id { get; init; }
        public string InvoiceNumber { get; init; } = string.Empty;
        public int PatientId { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public InvoiceStatus Status { get; init; }
        public decimal SubTotal { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal DiscountPercent { get; init; }
        public decimal TaxPercent { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal PaidAmount { get; init; }
        public decimal OutstandingBalance { get; init; }
        public string? Notes { get; init; }
        public DateOnly? DueDate { get; init; }
        public DateTimeOffset? IssuedAt { get; init; }
        public DateTimeOffset? PaidAt { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public List<LineItemResultDto> LineItems { get; init; } = new();
        public List<PaymentResultDto> Payments { get; init; } = new();
    }
}

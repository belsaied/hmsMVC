using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.BillingModule.Requests
{
    public record CreateInvoiceRequest
    {
        [Required]
        public int PatientId { get; init; }

        public int? AppointmentId { get; init; }

        [MaxLength(500)]
        public string? Notes { get; init; }

        public DateOnly? DueDate { get; init; }

        public List<AddLineItemRequest>? LineItems { get; init; }
    }
}

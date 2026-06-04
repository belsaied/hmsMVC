using HMS.DAL.Models.Enums.BillingEnums;
using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.BillingModule.Requests
{
    public record AddLineItemRequest
    {
        [Required]
        [MaxLength(300)]
        public string Description { get; init; } = string.Empty;

        [Required]
        public LineItemType LineItemType { get; init; }

        public string? ReferenceId { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; init; } = 1;

        [Range(0.0001, double.MaxValue, ErrorMessage = "UnitPrice must be greater than zero.")]
        public decimal UnitPrice { get; init; }
    }
}

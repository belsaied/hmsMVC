using HMS.DAL.Models.Enums.BillingEnums;
using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.BillingModule.Requests
{
    public class AddLineItemRequest
    {
        [Required]
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        public LineItemType LineItemType { get; set; }

        public string? ReferenceId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;

        [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "UnitPrice must be greater than zero.")]
        public decimal UnitPrice { get; set; }
    }
}

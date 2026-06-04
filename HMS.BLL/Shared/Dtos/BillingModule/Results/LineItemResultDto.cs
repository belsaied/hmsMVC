using HMS.DAL.Models.Enums.BillingEnums;

namespace HMS.BLL.Shared.Dtos.BillingModule.Results
{
    public record LineItemResultDto
    {
        public Guid Id { get; init; }
        public string Description { get; init; } = string.Empty;
        public LineItemType LineItemType { get; init; }
        public string? ReferenceId { get; init; }
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal Total { get; init; }
    }
}

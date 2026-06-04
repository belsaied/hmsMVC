using HMS.DAL.Models.Enums.BillingEnums;

namespace HMS.BLL.Shared.Parameters
{
    public class InvoiceFilterParameters
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 50;

        public InvoiceStatus? Status { get; set; }
        public int? PatientId { get; set; }
        public int PageIndex { get; set; } = 1;

        private int _pageSize = DefaultPageSize;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}

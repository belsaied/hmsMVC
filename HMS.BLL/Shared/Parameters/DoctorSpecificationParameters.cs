using HMS.DAL.Models.Enums.DoctorEnums;

namespace HMS.BLL.Shared.Parameters
{
    public class DoctorSpecificationParameters
    {
        private const int DefaultPageSize = 5;
        private const int MaxPageSize = 20;

        public string? Search { get; set; }
        public DoctorStatus? Status { get; set; }
        public string? Specialization { get; set; }
        public int? DepartmentId { get; set; }
        public int PageIndex { get; set; } = 1;

        private int _PageSize = DefaultPageSize;
        public int PageSize 
        {
            get => _PageSize;
            set => _PageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}

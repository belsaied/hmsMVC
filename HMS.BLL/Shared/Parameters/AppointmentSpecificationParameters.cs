using HMS.DAL.Models.Enums.AppointmentEnums;

namespace HMS.BLL.Shared.Parameters
{
    public class AppointmentSpecificationParameters
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 50;

        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public AppointmentStatus? Status { get; set; }
        public int PageIndex { get; set; } = 1;

        private int _pageSize = DefaultPageSize;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}

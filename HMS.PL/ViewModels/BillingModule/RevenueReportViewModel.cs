using HMS.BLL.Shared.Dtos.BillingModule.Results;

namespace HMS.PL.ViewModels.BillingModule
{
    public class RevenueReportViewModel
    {
        public RevenueReportResultDto? Report { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? DepartmentId { get; set; }
        public int? DoctorId { get; set; }
    }
}

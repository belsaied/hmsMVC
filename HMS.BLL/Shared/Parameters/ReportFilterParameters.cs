namespace HMS.BLL.Shared.Parameters
{
    public class ReportFilterParameters
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? DepartmentId { get; set; }
        public int? DoctorId { get; set; }
    }
}

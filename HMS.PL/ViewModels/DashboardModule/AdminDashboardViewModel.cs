using HMS.BLL.Shared.Dtos.BillingModule.Results;
using HMS.BLL.Shared.Dtos.WardBedModule.AdmissionDtos;
using HMS.BLL.Shared.Dtos.WardBedModule.WardDtos;

namespace HMS.PL.ViewModels.DashboardModule
{
    public class AdminDashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int ActiveDoctors { get; set; }
        public int TodaysAppointments { get; set; }
        public int ActiveAdmissions { get; set; }
        public int AvailableBeds { get; set; }
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int TotalDepartments { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TotalInvoiced { get; set; }
        public decimal TotalOutstanding { get; set; }
        public List<RevenueByGroupResultDto> RevenueByService { get; set; } = new();

        public List<InvoiceSummaryResultDto> OverdueInvoices { get; set; } = new();
        public int OverdueCount { get; set; }

        public List<WardOccupancySummaryDto> WardOccupancy { get; set; } = new();

        public List<AdmissionResultDto> RecentAdmissions { get; set; } = new();

        public int ScheduledCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
    }
}

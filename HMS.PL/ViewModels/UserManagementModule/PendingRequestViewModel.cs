namespace HMS.PL.ViewModels.UserManagementModule
{
    public class PendingRequestViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RequestedRole { get; set; } = string.Empty;
        public string? LicenseNumber { get; set; }
        public string? Message { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class PendingRequestPageViewModel
    {
        public IEnumerable<PendingRequestViewModel> Requests { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}

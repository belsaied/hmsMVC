namespace HMS.PL.ViewModels.UserManagementModule
{
    public class AuditLogEntryViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class AuditLogPageViewModel
    {
        public IEnumerable<AuditLogEntryViewModel> Entries { get; set; } = [];
        public string? UserIdFilter { get; set; }
        public string? ActionFilter { get; set; }
        public int PageIndex { get; set; } = 1;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}

namespace HMS.DAL.Models.IdentityModule
{
    public class PendingAccountRequest
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RequestedRole { get; set; } = string.Empty;
        public string? LicenseNumber { get; set; }
        public string? Message { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
    }
}

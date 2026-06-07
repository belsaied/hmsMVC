namespace HMS.PL.ViewModels.UserManagementModule
{
    public class UserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = [];
        public string PrimaryRole => Roles.FirstOrDefault() ?? "Unknown";
        public bool IsEmailVerified { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public DateTime? EmailVerificationExpiry { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}

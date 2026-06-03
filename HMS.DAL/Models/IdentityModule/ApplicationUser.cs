using Microsoft.AspNetCore.Identity;

namespace HMS.DAL.Models.IdentityModule
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";

        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationTokenHash { get; set; }
        public DateTime? EmailVerificationExpiry { get; set; }

        public string? PasswordResetTokenHash { get; set; }
        public DateTime? PasswordResetExpiry { get; set; }

        public string? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}

using HMS.DAL.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.DAL.Data.Configurations.UserManagementConfigs
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(x => x.FirstName)
    .HasMaxLength(100)
    .IsRequired();

            builder.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            // Token hash fields — long enough for SHA-256 Base64
            builder.Property(x => x.RefreshTokenHash)
                .HasMaxLength(512);

            builder.Property(x => x.EmailVerificationTokenHash)
                .HasMaxLength(512);

            builder.Property(x => x.PasswordResetTokenHash)
                .HasMaxLength(512);

            
            builder.HasIndex(x => x.RefreshTokenHash)
                .HasDatabaseName("IX_AspNetUsers_RefreshTokenHash");

            
            builder.HasIndex(x => x.EmailVerificationTokenHash)
                .HasDatabaseName("IX_AspNetUsers_EmailVerificationTokenHash");

            
            builder.HasIndex(x => x.PasswordResetTokenHash)
                .HasDatabaseName("IX_AspNetUsers_PasswordResetTokenHash");

            builder.HasIndex(x => x.DoctorId)
                .HasDatabaseName("IX_AspNetUsers_DoctorId")
                .IsUnique()
                .HasFilter("[DoctorId] IS NOT NULL");  

            builder.HasIndex(x => x.PatientId)
                .HasDatabaseName("IX_AspNetUsers_PatientId")
                .IsUnique()
                .HasFilter("[PatientId] IS NOT NULL");

            // FullName is computed in C# — not stored
            builder.Ignore(x => x.FullName);
        }
    }
}

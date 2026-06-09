using HMS.DAL.Models.IdentityModule;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HMS.DAL.Data.Identity
{
    public class IdentityHospitalDbContext : IdentityDbContext<ApplicationUser>
    {
        public IdentityHospitalDbContext(DbContextOptions<IdentityHospitalDbContext> options)
            : base(options) { }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<PendingAccountRequest> PendingAccountRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
        }
    }
}

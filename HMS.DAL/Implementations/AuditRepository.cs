using HMS.DAL.Contracts;
using HMS.DAL.Models.IdentityModule;
using HMS.DAL.Data.Identity;

namespace HMS.DAL.Implementations
{
    public class AuditRepository(IdentityHospitalDbContext _identityContext) : IAuditRepository
    {
        public async Task LogAsync(string userId, string action, string? details = null, string? ip = null)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                Details = details,
                IpAddress = ip,
            };
            await _identityContext.AuditLogs.AddAsync(log);
            await _identityContext.SaveChangesAsync();
        }
    }
}

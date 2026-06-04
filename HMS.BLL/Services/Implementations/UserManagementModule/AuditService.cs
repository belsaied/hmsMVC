using HMS.DAL.Contracts;
using Services.Abstraction.Contracts;

namespace HMS.BLL.Services.Implementations.UserManagementModule
{
    public class AuditService(IAuditRepository _auditRepository) : IAuditService
    {
        public Task LogAsync(string userId, string action, string? details = null, string? ip = null)
            => _auditRepository.LogAsync(userId, action, details, ip);
    }
}

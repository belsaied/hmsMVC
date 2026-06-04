namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface IAuditService
    {
        Task LogAsync(string userId, string action, string? details = null, string? ip = null);

    }
}

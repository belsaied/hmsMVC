using Hangfire.Dashboard;

namespace HMS.PL.Factories
{
    /// <summary>
    /// Allows access to the Hangfire dashboard only for
    /// authenticated users with the "Admin" role.
    /// Usage in Program.cs:
    ///   app.UseHangfireDashboard("/hangfire", new DashboardOptions
    ///   {
    ///       Authorization = [new HangfireAdminAuthorizationFilter()]
    ///   });
    /// </summary>
    public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity?.IsAuthenticated == true
                && httpContext.User.IsInRole("Admin");
        }
    }
}

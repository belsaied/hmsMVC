using Hangfire;
using HMS.PL.Extensions;
using HMS.PL.Factories;
using HMS.PL.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<EnumValidationFilter>();
});

// Copy from API's AddInfrastructureServices extension
builder.Services.AddInfrastructureServices(builder.Configuration);

// Copy from API's AddCoreServices extension
builder.Services.AddCoreServices(builder.Configuration);

builder.Services.AddMemoryCache();

var app = builder.Build();

// ── Middleware ─────────────────────────────────────────
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseHangfireDashboard("/hangfire");
await app.SeedDatabaseAsync();
app.UseExceptionHandlingMiddlewares();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.RegisterBillingRecurringJobs();
app.UseWebSockets();

// ── SignalR Hubs ───────────────────────────────────────
app.MapHub<AppointmentHub>("/hubs/appointments");
app.MapHub<WardHub>("/hubs/beds");
app.MapHub<NotificationHub>("/hubs/notifications");

// ── MVC Route ──────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
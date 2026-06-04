// HMS.PL/Program.cs
using Hangfire;
using HMS.PL.Extensions;
using HMS.PL.Factories;
using HMS.PL.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ── MVC + JSON ────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<EnumValidationFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new StrictEnumConverterFactory());
    options.JsonSerializerOptions.Converters.Add(new FlexibleDateTimeConverter());
});

// ── Infrastructure (DBs, Redis, Hangfire, SignalR, Identity, JWT) ─────────
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── Core BLL Services + ServiceManager ────────────────────────────────────
builder.Services.AddCoreServices(builder.Configuration);

// ── Notification senders (Email/SMS/Push) ─────────────────────────────────
builder.Services.AddNotificationServices(builder.Configuration);

builder.Services.AddMemoryCache();

var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseSwaggerMiddlewares();           // swagger available in dev
app.UseHangfireDashboard("/hangfire");

await app.SeedDatabaseAsync();

app.UseExceptionHandlingMiddlewares();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("DevPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.RegisterBillingRecurringJobs();
app.UseWebSockets();

// ── SignalR Hubs ───────────────────────────────────────────────────────────
app.MapHub<AppointmentHub>("/hubs/appointments");
app.MapHub<WardHub>("/hubs/beds");
app.MapHub<NotificationHub>("/hubs/notifications");

// ── MVC Route ──────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
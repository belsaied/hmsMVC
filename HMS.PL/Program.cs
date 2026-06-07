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

// ── Raw body buffering for Stripe webhook ────────────────────────────────
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/Payments/StripeWebhook"))
        context.Request.EnableBuffering();
    await next();
});

// ── MVC Routes ────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "stripe-webhook",
    pattern: "Payments/StripeWebhook",
    defaults: new { controller = "Payments", action = "StripeWebhook" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
app.Run();
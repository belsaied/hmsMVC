// HMS.PL/Extensions/InfrastructureServiceExtensions.cs
using HMS.DAL.Contracts;
using HMS.DAL.Data.DbContexts;
using HMS.DAL.Data.Identity;
using HMS.DAL.Implementations;
using HMS.DAL.Senders;
using HMS.PL.Hubs;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.ServicesAbstraction.Contracts.BillingService;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.ServicesAbstraction.Contracts.WardBedService;
using HMS.BLL.Services.Implementations;
using HMS.BLL.Services.Implementations.AppointmentModule;
using HMS.BLL.Services.Implementations.BillingModule;
using HMS.BLL.Services.Implementations.DoctorModule;
using HMS.BLL.Services.Implementations.MedicalRecordModule;
using HMS.BLL.Services.Implementations.NotificationModule;
using HMS.BLL.Services.Implementations.PatientModule;
using HMS.BLL.Services.Implementations.UserManagementModule;
using HMS.BLL.Services.Implementations.WardBedModule;
using HMS.BLL.Shared.Common;
using HMS.BLL.Shared.Common.NotificationSettings;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using Services.Implementations.BillingModule;
using Services.Implementations.MedicalRecordModule;

namespace HMS.PL.Extensions
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── SQL Server (HMS DB) ────────────────────────────────────────────
            services.AddDbContext<HospitalDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("HospitalConnection")));

            // ── SQL Server (Identity DB) ───────────────────────────────────────
            services.AddDbContext<IdentityHospitalDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("IdentityConnection")));

            // ── ASP.NET Core Identity ─────────────────────────────────────────
            services.AddIdentity<HMS.DAL.Models.IdentityModule.ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<IdentityHospitalDbContext>()
            .AddDefaultTokenProviders();

            // ── JWT ───────────────────────────────────────────────────────────
            services.Configure<JwtOptions>(configuration.GetSection("JwtSettings"));
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtOptions>()!;
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                };
            });

            // ── Redis ─────────────────────────────────────────────────────────
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
            services.AddScoped<ICacheRepository, CacheRepository>();

            // ── Hangfire ──────────────────────────────────────────────────────
            services.AddHangfire(cfg => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));
            services.AddHangfireServer();

            // ── DAL ───────────────────────────────────────────────────────────
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<IDataSeeding, DataSeeding>();
            services.AddScoped<IdentityDataSeeding>();

            // ── SignalR ───────────────────────────────────────────────────────
            services.AddSignalR();

            return services;
        }
    }
}
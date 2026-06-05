// HMS.PL/Extensions/InfrastructureServiceExtensions.cs
using Hangfire;
using HMS.BLL.Shared.Common;
using HMS.DAL.Contracts;
using HMS.DAL.Data.DbContexts;
using HMS.DAL.Data.Identity;
using HMS.DAL.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

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
            // Redis is optional — if not available, use in-memory fallback
            try
            {
                var redisConn = configuration.GetConnectionString("Redis") ?? "localhost:6379";
                var multiplexer = ConnectionMultiplexer.Connect(redisConn);
                services.AddSingleton<IConnectionMultiplexer>(multiplexer);
                services.AddScoped<ICacheRepository, CacheRepository>();
            }
            catch
            {
                // Redis unavailable — register a no-op cache so the app still starts
                services.AddSingleton<IConnectionMultiplexer>(_ =>
                    ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false"));
                services.AddScoped<ICacheRepository, CacheRepository>();
            }

            // ── Hangfire — ensure DB exists before registering ─────────────────
            EnsureHangfireDatabaseExists(
                configuration.GetConnectionString("HangfireConnection")!);

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

        /// <summary>
        /// Creates the Hangfire database if it does not already exist.
        /// This prevents the "Cannot open database" error on first run.
        /// </summary>
        private static void EnsureHangfireDatabaseExists(string hangfireConnectionString)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(hangfireConnectionString);
                var databaseName = builder.InitialCatalog;

                // Connect to master to create the DB
                builder.InitialCatalog = "master";

                using var connection = new SqlConnection(builder.ConnectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"""
                    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{databaseName}')
                    BEGIN
                        CREATE DATABASE [{databaseName}];
                    END
                    """;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Log but don't crash — Hangfire will surface its own error if DB is truly unavailable
                Console.WriteLine($"[Hangfire DB Init] Could not ensure database exists: {ex.Message}");
            }
        }
    }
}
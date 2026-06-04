using HMS.PL.Factories;
using Microsoft.AspNetCore.Mvc;

namespace HMS.PL.Extensions
{
    public static class WebApiServiceExtensions
    {
        public static IServiceCollection AddWebApiServices(
            this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<EnumValidationFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new StrictEnumConverterFactory());
                options.JsonSerializerOptions.Converters.Add(new FlexibleDateTimeConverter());
            });

            services.AddEndpointsApiExplorer();
            services.AddOpenApi();
            services.AddMemoryCache();

            services.AddSwaggerGen(options =>
            {
                options.UseInlineDefinitionsForEnums();
            });

            services.AddCors(options =>
            {
                options.AddPolicy("DevPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory =
                    ApiResponseFactory.GenerateApiValidationResponse;
            });
            return services;
        }
    }
}
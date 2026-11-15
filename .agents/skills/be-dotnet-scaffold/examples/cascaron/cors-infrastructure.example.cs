// Ubicado en src/Presentation/Web.API/Infrastructure/CorsExtensions.cs

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.API.Infrastructure;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string policyName)
    {
        var origins = configuration.GetSection("FrontendUrls").Get<string[]>();
        if (origins is null || origins.Length == 0)
        {
            var singleOrigin = configuration["FrontendUrl"] ?? "http://localhost:4200";
            origins = [singleOrigin];
        }

        services.AddCors(options =>
        {
            options.AddPolicy(policyName, policy =>
            {
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}

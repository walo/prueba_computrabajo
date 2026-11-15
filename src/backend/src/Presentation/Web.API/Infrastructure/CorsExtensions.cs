namespace Web.API.Infrastructure;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string policyName)
    {
        var origins = ResolveAllowedOrigins(configuration);
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

    private static string[] ResolveAllowedOrigins(IConfiguration configuration)
    {
        var rawEnvOrigins = configuration["CORS_ALLOWED_ORIGINS"];
        if (!string.IsNullOrWhiteSpace(rawEnvOrigins))
        {
            return rawEnvOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        var configuredOrigins = configuration.GetSection("FrontendUrls").Get<string[]>();
        if (configuredOrigins is not null && configuredOrigins.Length > 0)
        {
            return configuredOrigins;
        }
        var fallback = configuration["FrontendUrl"] ?? "http://localhost:4200";

        return [fallback];
    }
}

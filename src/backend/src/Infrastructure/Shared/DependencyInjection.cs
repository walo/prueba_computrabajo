using Application.Common.Interfaces;
using Infrastructure.Shared.Caching;
using Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Infrastructure.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        RegisterRedisConnection(services, configuration);
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }

    private static void RegisterRedisConnection(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = ResolveRedisConnectionString(configuration);
        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            return;
        }
        var options = ConfigurationOptions.Parse(redisConnection);
        options.AbortOnConnectFail = false;
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(options));
    }

    private static string? ResolveRedisConnectionString(IConfiguration configuration)
    {
        var configured = configuration.GetConnectionString("Redis");
        if (configured is not null)
        {
            return configured;
        }
        var redisPort = configuration["REDIS_PORT"];

        return string.IsNullOrWhiteSpace(redisPort) ? null : $"localhost:{redisPort}";
    }
}

using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Tests.IntegrationTests.Common;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"IntegrationTestDb_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Redis"] = string.Empty,
                ["JWT_SECRET_KEY"] = "TestSecretKeyForRedarborIntegrationTests2026WithMinimum32Bytes!",
                ["JWT_ISSUER"] = "RedarborInventoryApi",
                ["JWT_AUDIENCE"] = "RedarborInventoryClient",
                ["JWT_ACCESS_TOKEN_MINUTES"] = "15"
            });
        });
        builder.ConfigureServices(ConfigureTestPersistence);
    }

    private void ConfigureTestPersistence(IServiceCollection services)
    {
        var descriptorsToRemove = services
            .Where(descriptor =>
                descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                descriptor.ServiceType == typeof(IConnectionMultiplexer) ||
                (descriptor.ServiceType.IsGenericType &&
                 descriptor.ServiceType.GetGenericTypeDefinition().FullName?.Contains("IDbContextOptionsConfiguration") == true))
            .ToList();
        foreach (var descriptor in descriptorsToRemove)
        {
            services.Remove(descriptor);
        }
        var testSqlConnection = Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION");
        if (!string.IsNullOrWhiteSpace(testSqlConnection))
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(testSqlConnection));

            return;
        }
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
    }
}

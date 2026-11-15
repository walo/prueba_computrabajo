using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Seeding;

namespace Web.API.Infrastructure;

public static class DatabaseSeedExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeding");
        await InitialDataSeeder.SeedAsync(new DatabaseSeedRequest(dbContext, passwordHasher, logger));
    }
}

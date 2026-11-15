using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence.Connections;

public static class ConnectionStringResolver
{
    public static string Resolve(IConfiguration configuration)
    {
        var configured = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }
        var host = configuration["DB_HOST"] ?? "localhost";
        var port = configuration["DB_PORT"] ?? "1433";
        var database = configuration["DB_NAME"] ?? "RedarborInventoryDb";
        var user = configuration["DB_USER"] ?? "sa";
        var password = configuration["DB_PASSWORD"]
            ?? throw new InvalidOperationException("Database connection string or DB_PASSWORD is not configured.");

        return $"Server={host},{port};Database={database};User Id={user};Password={password};TrustServerCertificate=True;Encrypt=False;";
    }
}

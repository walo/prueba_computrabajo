// NOTA: Cada librería define su propio DependencyInjection.cs independiente:
// - Core/Application/DependencyInjection.cs -> AddApplication()
// - Infrastructure/Shared/DependencyInjection.cs -> AddSharedInfrastructure(IConfiguration)
// - Infrastructure/Persistence/DependencyInjection.cs -> AddPersistenceInfrastructure(IConfiguration)
// - Infrastructure/Authentication/DependencyInjection.cs -> AddAuthenticationInfrastructure()
// - Infrastructure/ExternalServices/DependencyInjection.cs -> AddExternalServicesInfrastructure(IConfiguration)

using Application.Common.Interfaces;
using Domain.Primitives;
using Infrastructure.Persistence.Finders;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuthFinder, AuthFinder>();
        services.AddScoped<IUserFinder, UserFinder>();

        return services;
    }
}

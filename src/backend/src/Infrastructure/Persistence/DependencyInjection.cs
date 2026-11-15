using Application.Common.Interfaces;
using Domain.Primitives;
using Infrastructure.Persistence.Connections;
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
        var connectionString = ConnectionStringResolver.Resolve(configuration);
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServer =>
                sqlServer.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuthFinder, AuthFinder>();
        services.AddScoped<IUserFinder, UserFinder>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryFinder, CategoryFinder>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductFinder, ProductFinder>();
        services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
        services.AddScoped<IInventoryMovementFinder, InventoryMovementFinder>();

        return services;
    }
}

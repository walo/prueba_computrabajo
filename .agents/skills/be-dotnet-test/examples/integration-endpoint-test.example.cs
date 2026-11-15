using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Features.Auth.DTOs;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Tests.IntegrationTests.Examples;

// --- Archivo: Common/CustomWebApplicationFactory.cs ---
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
                ["ConnectionStrings:Redis"] = string.Empty
            });
        });

        builder.ConfigureServices(services =>
        {
            var testPostgresConnection = Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION");

            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(IConnectionMultiplexer) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition().FullName?.Contains("IDbContextOptionsConfiguration") == true))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            if (!string.IsNullOrWhiteSpace(testPostgresConnection))
            {
                services.AddDbContext<AppDbContext>(options => options.UseNpgsql(testPostgresConnection));
            }
            else
            {
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            }
        });
    }
}

// --- Archivo: Common/IntegrationTestBase.cs ---
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<AuthTokensResponse> AuthenticateAsAdminAsync()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("admin@enterprise.local", "Admin123*"));

        response.EnsureSuccessStatusCode();
        var tokens = (await response.Content.ReadFromJsonAsync<AuthTokensResponse>())!;
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        return tokens;
    }
}

// --- Archivo: Features/Users/UsersEndpointsIntegrationTests.cs ---
public sealed class UsersEndpointsIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Should_Return200Ok_When_AdminRequestsUsersEndpoint()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Return401Unauthorized_When_NoBearerTokenIsProvided()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}


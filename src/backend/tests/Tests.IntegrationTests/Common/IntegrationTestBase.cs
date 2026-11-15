using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Features.Auth.DTOs;

namespace Tests.IntegrationTests.Common;

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
        return await AuthenticateWithCredentialsAsync(new LoginRequest("admin@enterprise.local", "Admin123*"));
    }

    protected async Task<AuthTokensResponse> AuthenticateAsUserAsync()
    {
        return await AuthenticateWithCredentialsAsync(new LoginRequest("user@enterprise.local", "User123*"));
    }

    protected async Task<AuthTokensResponse> AuthenticateWithCredentialsAsync(LoginRequest credentials)
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", credentials);
        response.EnsureSuccessStatusCode();
        var tokens = (await response.Content.ReadFromJsonAsync<AuthTokensResponse>())!;
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        return tokens;
    }
}

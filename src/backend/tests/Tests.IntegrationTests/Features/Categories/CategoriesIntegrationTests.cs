using System.Net;
using System.Net.Http.Json;
using Application.Features.Categories.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Tests.IntegrationTests.Common;

namespace Tests.IntegrationTests.Features.Categories;

public sealed class CategoriesIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CategoriesEndpoints_Should_EnforceAuthenticationAndPbac()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var anonymousResponse = await Client.GetAsync("/api/v1/categories");
        anonymousResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await AuthenticateAsUserAsync();
        var readResponse = await Client.GetAsync("/api/v1/categories");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var forbiddenCreate = await Client.PostAsJsonAsync(
            "/api/v1/categories",
            new CreateCategoryRequest("unauth-cat", "Forbidden Category", null));
        forbiddenCreate.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CategoriesCrudLifecycle_Should_Succeed_When_AuthenticatedAsAdmin()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await AuthenticateAsAdminAsync();
        var createResponse = await Client.PostAsJsonAsync(
            "/api/v1/categories",
            new CreateCategoryRequest($"HDW-{suffix}", $"Hardware {suffix}", "Components and peripherals"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await createResponse.Content.ReadFromJsonAsync<CategoryResponse>())!;
        created.Code.Should().Be($"hdw-{suffix}");
        created.Name.Should().Be($"Hardware {suffix}");
        created.IsActive.Should().BeTrue();
        await VerifyPagedReadAndUpdateAsync(created.Id, suffix);
        await VerifyDeleteAndNotFoundAsync(created.Id);
    }

    [Fact]
    public async Task Create_Should_ReturnConflict_When_CodeOrNameAlreadyExists()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await AuthenticateAsAdminAsync();
        var initialResponse = await Client.PostAsJsonAsync(
            "/api/v1/categories",
            new CreateCategoryRequest($"DUP-{suffix}", $"Unique Drinks {suffix}", null));
        initialResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await initialResponse.Content.ReadFromJsonAsync<CategoryResponse>())!;
        var duplicateCodeResponse = await Client.PostAsJsonAsync(
            "/api/v1/categories",
            new CreateCategoryRequest($"dup-{suffix}", $"Different Name {suffix}", null));
        duplicateCodeResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = (await duplicateCodeResponse.Content.ReadFromJsonAsync<ProblemDetails>())!;
        problem.Title.Should().Be("Category.DuplicateCode");
        await Client.DeleteAsync($"/api/v1/categories/{created.Id}");
    }

    private async Task VerifyPagedReadAndUpdateAsync(Guid categoryId, string suffix)
    {
        var listResponse = await Client.GetFromJsonAsync<PagedCategoriesResponse>(
            $"/api/v1/categories?page=1&pageSize=10&search={suffix}&isActive=true");
        listResponse.Should().NotBeNull();
        listResponse!.Items.Should().Contain(item => item.Id == categoryId);
        var updateResponse = await Client.PutAsJsonAsync(
            $"/api/v1/categories/{categoryId}",
            new UpdateCategoryRequest($"hdw-{suffix}", $"Hardware Pro {suffix}", "Updated description", false));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await updateResponse.Content.ReadFromJsonAsync<CategoryResponse>())!;
        updated.Name.Should().Be($"Hardware Pro {suffix}");
        updated.IsActive.Should().BeFalse();
    }

    private async Task VerifyDeleteAndNotFoundAsync(Guid categoryId)
    {
        var deleteResponse = await Client.DeleteAsync($"/api/v1/categories/{categoryId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getDeletedResponse = await Client.GetAsync($"/api/v1/categories/{categoryId}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

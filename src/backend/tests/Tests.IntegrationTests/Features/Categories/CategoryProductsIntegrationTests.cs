using System.Net;
using System.Net.Http.Json;
using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using Application.Features.Categories.DTOs;
using Application.Features.Products.DTOs;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Tests.IntegrationTests.Common;

namespace Tests.IntegrationTests.Features.Categories;

public sealed class CategoryProductsIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetProductsByCategory_Should_EnforceAuthenticationAndPbac()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await AuthenticateAsAdminAsync();
        var category = await CreateCategoryAsync(new CreateCategoryRequest($"pbac-{suffix}", $"PBAC {suffix}", null));
        Client.DefaultRequestHeaders.Authorization = null;
        var anonymousResponse = await Client.GetAsync($"/api/v1/categories/{category.Id}/products");
        anonymousResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var restrictedCredentials = await CreateUserWithoutProductReadAsync(suffix);
        await AuthenticateWithCredentialsAsync(restrictedCredentials);
        var forbiddenResponse = await Client.GetAsync($"/api/v1/categories/{category.Id}/products");
        forbiddenResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        await AuthenticateAsUserAsync();
        var allowedResponse = await Client.GetAsync($"/api/v1/categories/{category.Id}/products");
        allowedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductsByCategory_Should_ReturnFilteredAndPagedProductsForCategoryOnly()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await AuthenticateAsAdminAsync();
        var targetCategory = await CreateCategoryAsync(new CreateCategoryRequest($"tgt-{suffix}", $"Target {suffix}", null));
        var otherCategory = await CreateCategoryAsync(new CreateCategoryRequest($"oth-{suffix}", $"Other {suffix}", null));
        await CreateProductAsync(new CreateProductRequest($"low-{suffix}", $"LowStock {suffix}", null, targetCategory.Id, 100m, 2, 5));
        await CreateProductAsync(new CreateProductRequest($"nrm-{suffix}", $"Normal {suffix}", null, targetCategory.Id, 200m, 20, 5));
        var toDeactivate = await CreateProductAsync(new CreateProductRequest($"ina-{suffix}", $"Inactive {suffix}", null, targetCategory.Id, 300m, 1, 5));
        await DeactivateProductAsync(toDeactivate);
        await CreateProductAsync(new CreateProductRequest($"ext-{suffix}", $"External {suffix}", null, otherCategory.Id, 400m, 1, 5));
        await VerifyCategoryIsolationAndPaginationAsync(targetCategory.Id);
        await VerifyFilteredCategoryProductsAsync(targetCategory, suffix);
    }

    [Fact]
    public async Task GetProductsByCategory_Should_HandleInactiveEmptyMissingAndInvalidQueries()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await AuthenticateAsAdminAsync();
        var inactiveCategory = await CreateCategoryAsync(new CreateCategoryRequest($"inact-{suffix}", $"Inactive Cat {suffix}", null));
        await CreateProductAsync(new CreateProductRequest($"hist-{suffix}", $"Historic {suffix}", null, inactiveCategory.Id, 50m, 4, 2));
        await DeactivateCategoryAsync(inactiveCategory);
        var inactiveResult = await Client.GetFromJsonAsync<PagedProductsResponse>($"/api/v1/categories/{inactiveCategory.Id}/products");
        inactiveResult!.TotalCount.Should().Be(1);
        var emptyCategory = await CreateCategoryAsync(new CreateCategoryRequest($"emp-{suffix}", $"Empty Cat {suffix}", null));
        var emptyResult = await Client.GetFromJsonAsync<PagedProductsResponse>($"/api/v1/categories/{emptyCategory.Id}/products");
        emptyResult!.Items.Should().BeEmpty();
        emptyResult.TotalCount.Should().Be(0);
        await AssertMissingAndInvalidQueriesAsync(emptyCategory.Id);
    }

    private async Task VerifyCategoryIsolationAndPaginationAsync(Guid categoryId)
    {
        var allInTarget = await Client.GetFromJsonAsync<PagedProductsResponse>($"/api/v1/categories/{categoryId}/products?page=1&pageSize=10");
        allInTarget.Should().NotBeNull();
        allInTarget!.TotalCount.Should().Be(3);
        allInTarget.Items.Should().OnlyContain(item => item.CategoryId == categoryId);
        var paged = await Client.GetFromJsonAsync<PagedProductsResponse>($"/api/v1/categories/{categoryId}/products?page=1&pageSize=1");
        paged.Should().NotBeNull();
        paged!.Items.Should().HaveCount(1);
        paged.TotalCount.Should().Be(3);
    }

    private async Task VerifyFilteredCategoryProductsAsync(CategoryResponse targetCategory, string suffix)
    {
        var filtered = await Client.GetFromJsonAsync<PagedProductsResponse>(
            $"/api/v1/categories/{targetCategory.Id}/products?page=1&pageSize=10&search=LOW-{suffix}&isActive=true&lowStockOnly=true");
        filtered.Should().NotBeNull();
        filtered!.TotalCount.Should().Be(1);
        var match = filtered.Items.Single();
        match.IsLowStock.Should().BeTrue();
        match.CategoryCode.Should().Be(targetCategory.Code);
        match.CategoryName.Should().Be(targetCategory.Name);
    }

    private async Task AssertMissingAndInvalidQueriesAsync(Guid validCategoryId)
    {
        var missingResponse = await Client.GetAsync($"/api/v1/categories/{Guid.NewGuid()}/products");
        missingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var missingProblem = (await missingResponse.Content.ReadFromJsonAsync<ProblemDetails>())!;
        missingProblem.Title.Should().Be("Category.NotFound");
        var invalidResponse = await Client.GetAsync($"/api/v1/categories/{validCategoryId}/products?page=0&pageSize=101");
        invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var validationProblem = (await invalidResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>())!;
        validationProblem.Title.Should().Be("Validation.Failed");
    }

    private async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var response = await Client.PostAsJsonAsync("/api/v1/categories", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<CategoryResponse>())!;
    }

    private async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
    {
        var response = await Client.PostAsJsonAsync("/api/v1/products", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    private async Task DeactivateCategoryAsync(CategoryResponse category)
    {
        var response = await Client.PutAsJsonAsync(
            $"/api/v1/categories/{category.Id}",
            new UpdateCategoryRequest(category.Code, category.Name, category.Description, false));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task DeactivateProductAsync(ProductResponse product)
    {
        var response = await Client.PutAsJsonAsync(
            $"/api/v1/products/{product.Id}",
            new UpdateProductRequest(product.Sku, product.Name, product.Description, product.CategoryId, product.Price, product.MinStock, false));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<LoginRequest> CreateUserWithoutProductReadAsync(string suffix)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var role = Role.Create(new RoleCreationData($"NOPROD-{suffix}", $"No Product Read {suffix}", "Restricted role"));
        var email = $"noprod-{suffix}@enterprise.local";
        var password = "User123*";
        var user = User.Create(new UserCreationData($"Restricted {suffix}", email, passwordHasher.Hash(password), role.Id));
        dbContext.Roles.Add(role);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return new LoginRequest(email, password);
    }
}

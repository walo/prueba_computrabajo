using System.Net;
using System.Net.Http.Json;
using Application.Features.Categories.DTOs;
using Application.Features.Products.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Tests.IntegrationTests.Common;

namespace Tests.IntegrationTests.Features.Products;

public sealed class ProductsIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task ProductsEndpoints_Should_EnforceAuthenticationAndPbac()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var anonymousResponse = await Client.GetAsync("/api/v1/products");
        anonymousResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await AuthenticateAsUserAsync();
        var readResponse = await Client.GetAsync("/api/v1/products");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var forbiddenCreate = await Client.PostAsJsonAsync(
            "/api/v1/products",
            new CreateProductRequest("UNAUTH-01", "Blocked Product", null, Guid.NewGuid(), 10m, 0, 1));
        forbiddenCreate.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ProductsCrudLifecycle_Should_JoinCategoryAndEnforceDeletionRules()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await AuthenticateAsAdminAsync();
        var category = await CreateCategoryAsync(new CreateCategoryRequest($"cat-{suffix}", $"Tech {suffix}", null));
        var created = await CreateProductAsync(
            new CreateProductRequest($"sku-{suffix}", $"Device {suffix}", "Integration spec", category.Id, 199.50m, 0, 3));
        created.Sku.Should().Be($"SKU-{suffix}".ToUpperInvariant());
        created.CategoryCode.Should().Be($"cat-{suffix}");
        created.CategoryName.Should().Be($"Tech {suffix}");
        created.IsLowStock.Should().BeTrue();
        await VerifyCategoryDeleteBlockedAsync(category.Id);
        await VerifyPagedFilterAndUpdateAsync(created, suffix);
        await VerifyProductAndCategoryCleanupAsync(created.Id, category.Id);
    }

    [Fact]
    public async Task ProductRules_Should_RejectDuplicateSkuInactiveCategoryAndStockDeletion()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await AuthenticateAsAdminAsync();
        var activeCategory = await CreateCategoryAsync(new CreateCategoryRequest($"act-{suffix}", $"Active {suffix}", null));
        var inactiveCategory = await CreateCategoryAsync(new CreateCategoryRequest($"ina-{suffix}", $"Inactive {suffix}", null));
        await Client.PutAsJsonAsync(
            $"/api/v1/categories/{inactiveCategory.Id}",
            new UpdateCategoryRequest($"ina-{suffix}", $"Inactive {suffix}", null, false));
        var created = await CreateProductAsync(
            new CreateProductRequest($"dup-{suffix}", $"Stocked {suffix}", "Integration spec", activeCategory.Id, 50m, 5, 2));
        await AssertConflictAsync(
            Client.PostAsJsonAsync("/api/v1/products", new CreateProductRequest($"DUP-{suffix}", "Other", null, activeCategory.Id, 10m, 0, 1)),
            "Product.DuplicateSku");
        await AssertConflictAsync(
            Client.PostAsJsonAsync("/api/v1/products", new CreateProductRequest($"new-{suffix}", "InactiveCat", null, inactiveCategory.Id, 10m, 0, 1)),
            "Product.CategoryInactive");
        await AssertConflictAsync(
            Client.DeleteAsync($"/api/v1/products/{created.Id}"),
            "Product.CannotDeleteWithStockOrMovements");
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

    private async Task VerifyCategoryDeleteBlockedAsync(Guid categoryId)
    {
        var deleteCategoryResponse = await Client.DeleteAsync($"/api/v1/categories/{categoryId}");
        deleteCategoryResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = (await deleteCategoryResponse.Content.ReadFromJsonAsync<ProblemDetails>())!;
        problem.Title.Should().Be("Category.HasAssociatedProducts");
    }

    private async Task VerifyPagedFilterAndUpdateAsync(ProductResponse product, string suffix)
    {
        var listResponse = await Client.GetFromJsonAsync<PagedProductsResponse>(
            $"/api/v1/products?page=1&pageSize=10&categoryId={product.CategoryId}&lowStockOnly=true&search={suffix}");
        listResponse.Should().NotBeNull();
        listResponse!.Items.Should().Contain(item => item.Id == product.Id);
        var updateResponse = await Client.PutAsJsonAsync(
            $"/api/v1/products/{product.Id}",
            new UpdateProductRequest($"sku-{suffix}", $"Device Pro {suffix}", "Updated", product.CategoryId, 249.99m, 5, false));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await updateResponse.Content.ReadFromJsonAsync<ProductResponse>())!;
        updated.Name.Should().Be($"Device Pro {suffix}");
        updated.Stock.Should().Be(0);
        updated.IsActive.Should().BeFalse();
    }

    private async Task VerifyProductAndCategoryCleanupAsync(Guid productId, Guid categoryId)
    {
        var deleteProductResponse = await Client.DeleteAsync($"/api/v1/products/{productId}");
        deleteProductResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getDeletedResponse = await Client.GetAsync($"/api/v1/products/{productId}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var deleteCategoryResponse = await Client.DeleteAsync($"/api/v1/categories/{categoryId}");
        deleteCategoryResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static async Task AssertConflictAsync(Task<HttpResponseMessage> responseTask, string expectedTitle)
    {
        var response = await responseTask;
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
        problem.Title.Should().Be(expectedTitle);
    }
}

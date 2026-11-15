using System.Net;
using System.Net.Http.Json;
using Application.Features.Categories.DTOs;
using Application.Features.InventoryMovements.DTOs;
using Application.Features.Products.DTOs;
using Domain.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Tests.IntegrationTests.Common;

namespace Tests.IntegrationTests.Features.InventoryMovements;

public sealed class InventoryMovementsIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task InventoryMovementsEndpoints_Should_EnforceAuthenticationAndPbac()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var anonymousResponse = await Client.GetAsync("/api/v1/inventory-movements");
        anonymousResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await AuthenticateAsUserAsync();
        var readResponse = await Client.GetAsync("/api/v1/inventory-movements");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var forbiddenCreate = await Client.PostAsJsonAsync(
            "/api/v1/inventory-movements",
            new CreateInventoryMovementRequest(Guid.NewGuid(), InventoryMovementTypes.Entry, 5, "Blocked action"));
        forbiddenCreate.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task MovementsLifecycle_Should_UpdateProductStockAndBlockProductDeletion()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await AuthenticateAsAdminAsync();
        var category = await CreateCategoryAsync($"mov-{suffix}", $"MovCat {suffix}");
        var product = await CreateProductAsync(category.Id, $"mv-{suffix}", $"Tracked {suffix}", 5, 3);
        var entry = await CreateMovementAsync(product.Id, InventoryMovementTypes.Entry, 10, "Initial replenishment");
        entry.PreviousStock.Should().Be(5);
        entry.NewStock.Should().Be(15);
        entry.ProductSku.Should().Be($"MV-{suffix}".ToUpperInvariant());
        entry.PerformedByEmail.Should().NotBeNullOrWhiteSpace();
        var exit = await CreateMovementAsync(product.Id, InventoryMovementTypes.Exit, 13, "Order fulfillment");
        exit.PreviousStock.Should().Be(15);
        exit.NewStock.Should().Be(2);
        await VerifyProductStockAndKardexFiltersAsync(product.Id, exit.Id);
        await CreateMovementAsync(product.Id, InventoryMovementTypes.Exit, 2, "Zero out stock");
        await AssertConflictAsync(
            Client.DeleteAsync($"/api/v1/products/{product.Id}"),
            "Product.CannotDeleteWithStockOrMovements");
    }

    [Fact]
    public async Task MovementRules_Should_RejectInsufficientStockAndInactiveProduct()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await AuthenticateAsAdminAsync();
        var category = await CreateCategoryAsync($"rul-{suffix}", $"Rules {suffix}");
        var product = await CreateProductAsync(category.Id, $"rl-{suffix}", $"Constrained {suffix}", 4, 1);
        await AssertConflictAsync(
            Client.PostAsJsonAsync(
                "/api/v1/inventory-movements",
                new CreateInventoryMovementRequest(product.Id, InventoryMovementTypes.Exit, 10, "Too large exit")),
            "InventoryMovement.InsufficientStock");
        await Client.PutAsJsonAsync(
            $"/api/v1/products/{product.Id}",
            new UpdateProductRequest($"rl-{suffix}", $"Constrained {suffix}", null, category.Id, 20m, 1, false));
        await AssertConflictAsync(
            Client.PostAsJsonAsync(
                "/api/v1/inventory-movements",
                new CreateInventoryMovementRequest(product.Id, InventoryMovementTypes.Entry, 3, "Inactive intake")),
            "InventoryMovement.ProductInactive");
    }

    private async Task<CategoryResponse> CreateCategoryAsync(string code, string name)
    {
        var response = await Client.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest(code, name, null));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<CategoryResponse>())!;
    }

    private async Task<ProductResponse> CreateProductAsync(
        Guid categoryId,
        string sku,
        string name,
        int initialStock,
        int minStock)
    {
        var response = await Client.PostAsJsonAsync(
            "/api/v1/products",
            new CreateProductRequest(sku, name, "Tracked item", categoryId, 99m, initialStock, minStock));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    private async Task<InventoryMovementResponse> CreateMovementAsync(
        Guid productId,
        string type,
        int quantity,
        string reason)
    {
        var response = await Client.PostAsJsonAsync(
            "/api/v1/inventory-movements",
            new CreateInventoryMovementRequest(productId, type, quantity, reason));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<InventoryMovementResponse>())!;
    }

    private async Task VerifyProductStockAndKardexFiltersAsync(Guid productId, Guid movementId)
    {
        var product = (await Client.GetFromJsonAsync<ProductResponse>($"/api/v1/products/{productId}"))!;
        product.Stock.Should().Be(2);
        product.IsLowStock.Should().BeTrue();
        var single = (await Client.GetFromJsonAsync<InventoryMovementResponse>($"/api/v1/inventory-movements/{movementId}"))!;
        single.Id.Should().Be(movementId);
        var paged = (await Client.GetFromJsonAsync<PagedInventoryMovementsResponse>(
            $"/api/v1/inventory-movements?page=1&pageSize=10&productId={productId}&type=EXIT"))!;
        paged.Items.Should().ContainSingle(item => item.Id == movementId);
    }

    private static async Task AssertConflictAsync(Task<HttpResponseMessage> responseTask, string expectedTitle)
    {
        var response = await responseTask;
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
        problem.Title.Should().Be(expectedTitle);
    }
}

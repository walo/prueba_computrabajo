using Application.Features.Categories.Commands.CreateCategory;
using Application.Features.Categories.Commands.DeleteCategory;
using Application.Features.Categories.Commands.UpdateCategory;
using Application.Features.Categories.DTOs;
using Application.Features.Categories.Queries.GetCategories;
using Application.Features.Categories.Queries.GetCategoryById;
using Application.Features.Products.DTOs;
using Application.Features.Products.Queries.GetProductsByCategory;
using Domain.Constants;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.API.Attributes;

namespace Web.API.Controllers;

[Route("api/v1/categories")]
public sealed class CategoriesController(ISender sender) : ApiController
{
    [HttpGet]
    [HasPermission(Permissions.CategoriesRead)]
    [ProducesResponseType(typeof(PagedCategoriesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] CategoryFilter filter, CancellationToken ct)
    {
        var result = await sender.Send(new GetCategoriesQuery(filter), ct);

        return result.Match(Ok, HandleFailure);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.CategoriesRead)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetCategoryByIdQuery(id), ct);

        return result.Match(Ok, HandleFailure);
    }

    [HttpGet("{categoryId:guid}/products")]
    [HasPermission(Permissions.ProductsRead)]
    [ProducesResponseType(typeof(PagedProductsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductsByCategory(
        Guid categoryId,
        [FromQuery] CategoryProductsFilter filter,
        CancellationToken ct)
    {
        var request = new CategoryProductsQuery(categoryId, filter);
        var result = await sender.Send(new GetProductsByCategoryQuery(request), ct);

        return result.Match(Ok, HandleFailure);
    }

    [HttpPost]
    [HasPermission(Permissions.CategoriesCreate)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateCategoryCommand(request), ct);

        return result.Match(
            category => CreatedAtAction(nameof(GetById), new { id = category.Id }, category),
            HandleFailure);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.CategoriesUpdate)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateCategoryCommand(id, request), ct);

        return result.Match(Ok, HandleFailure);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.CategoriesDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteCategoryCommand(id), ct);

        return result.Match(_ => NoContent(), HandleFailure);
    }
}

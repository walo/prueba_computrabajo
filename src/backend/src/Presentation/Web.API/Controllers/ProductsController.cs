using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.DTOs;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProducts;
using Domain.Constants;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.API.Attributes;

namespace Web.API.Controllers;

[Route("api/v1/products")]
public sealed class ProductsController(ISender sender) : ApiController
{
    [HttpGet]
    [HasPermission(Permissions.Products.Read)]
    [ProducesResponseType(typeof(PagedProductsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] ProductFilter filter, CancellationToken ct)
    {
        var result = await sender.Send(new GetProductsQuery(filter), ct);

        return result.Match(Ok, HandleFailure);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Products.Read)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetProductByIdQuery(id), ct);

        return result.Match(Ok, HandleFailure);
    }

    [HttpPost]
    [HasPermission(Permissions.Products.Create)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateProductCommand(request), ct);

        return result.Match(
            product => CreatedAtAction(nameof(GetById), new { id = product.Id }, product),
            HandleFailure);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Products.Update)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateProductCommand(id, request), ct);

        return result.Match(Ok, HandleFailure);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Products.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteProductCommand(id), ct);

        return result.Match(_ => NoContent(), HandleFailure);
    }
}

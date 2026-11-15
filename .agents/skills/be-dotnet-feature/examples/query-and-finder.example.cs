using Application.Abstractions.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Items.DTOs;

public sealed record ItemDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool IsActive { get; init; }
}

namespace Application.Features.Items.Queries.GetItems;

// 1. Query inmutable
public sealed record GetItemsQuery(string? SearchTerm = null) : IRequest<ErrorOr<List<DTOs.ItemDto>>>;

// 2. Abstracción de Finder (Read Model optimizado sin IRepository ni tracking)
public interface IItemFinder
{
    Task<List<DTOs.ItemDto>> SearchAsync(string? searchTerm, CancellationToken cancellationToken = default);
}

// 3. QueryHandler desacoplado
public sealed class GetItemsQueryHandler : IRequestHandler<GetItemsQuery, ErrorOr<List<DTOs.ItemDto>>>
{
    private readonly IItemFinder _finder;

    public GetItemsQueryHandler(IItemFinder finder)
    {
        _finder = finder;
    }

    public async Task<ErrorOr<List<DTOs.ItemDto>>> Handle(GetItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _finder.SearchAsync(request.SearchTerm, cancellationToken);
        return items;
    }
}

// 4. Implementación en Infrastructure (con AsNoTracking)
namespace Infrastructure.Persistence.Finders;

public sealed class ItemFinder : Application.Features.Items.Queries.GetItems.IItemFinder
{
    private readonly ApplicationDbContext _context;

    public ItemFinder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Application.Features.Items.DTOs.ItemDto>> SearchAsync(
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Domain.Entities.Features.Item>()
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(i => i.Name.ToLower().Contains(term) || i.Code.ToLower().Contains(term));
        }

        return await query
            .Select(i => new Application.Features.Items.DTOs.ItemDto
            {
                Id = i.Id,
                Code = i.Code,
                Name = i.Name,
                Description = i.Description,
                Price = i.Price,
                IsActive = i.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}

using Domain.Entities.Features;
using Domain.Primitives;
using Domain.Repositories;
using Domain.Services;
using ErrorOr;
using MediatR;

namespace Application.Features.Items.Commands.CreateItem;

// 1. Command inmutable como record de C#
public sealed record CreateItemCommand(
    Guid CompanyId,
    string Code,
    string Name,
    string Description,
    decimal Price) : IRequest<ErrorOr<Guid>>;

// 2. CommandHandler desacoplado: NUNCA inyecta DbContext. Usa Repositorios, Domain Services y UnitOfWork.
public sealed class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, ErrorOr<Guid>>
{
    private readonly IItemRepository _itemRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IItemDomainService _itemDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateItemCommandHandler(
        IItemRepository itemRepository,
        ICompanyRepository companyRepository,
        IItemDomainService itemDomainService,
        IUnitOfWork unitOfWork)
    {
        _itemRepository = itemRepository;
        _companyRepository = companyRepository;
        _itemDomainService = itemDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        // 1. Validación de existencia previa de relaciones usando el Repositorio correspondiente
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);
        if (company is null || !company.IsActive)
        {
            return Error.NotFound("Company.NotFound", $"La empresa con ID '{request.CompanyId}' no existe o está inactiva.");
        }

        // 2. Validación de regla de negocio/unicidad reutilizable delegada al Domain Service
        var isUnique = await _itemDomainService.IsCodeUniqueAsync(request.Code, request.CompanyId, cancellationToken);
        if (!isUnique)
        {
            return Error.Conflict("Item.DuplicateCode", $"Ya existe un registro con el código '{request.Code}' para esta empresa.");
        }

        // 3. Creación mediante método de fábrica de dominio del agregado
        var item = Item.Create(
            companyId: request.CompanyId,
            code: request.Code,
            name: request.Name,
            description: request.Description,
            price: request.Price);

        // 4. Agregar al repositorio de agregados (Write Model)
        await _itemRepository.AddAsync(item, cancellationToken);

        // 5. Persistencia atómica + Despacho de DomainEvents vía IUnitOfWork
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}



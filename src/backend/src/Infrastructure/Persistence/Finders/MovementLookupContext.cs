namespace Infrastructure.Persistence.Finders;

public sealed record MovementLookupContext(
    IReadOnlyDictionary<Guid, (string Sku, string Name)> Products,
    IReadOnlyDictionary<Guid, string> Users);

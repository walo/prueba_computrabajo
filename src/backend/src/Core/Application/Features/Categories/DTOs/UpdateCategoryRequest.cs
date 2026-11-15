namespace Application.Features.Categories.DTOs;

public sealed record UpdateCategoryRequest(string Code, string Name, string? Description, bool IsActive);

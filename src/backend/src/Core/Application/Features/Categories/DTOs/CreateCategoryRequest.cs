namespace Application.Features.Categories.DTOs;

public sealed record CreateCategoryRequest(string Code, string Name, string? Description);

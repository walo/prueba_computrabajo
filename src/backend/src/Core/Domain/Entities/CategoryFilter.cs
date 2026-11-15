namespace Domain.Entities;

public sealed record CategoryFilter(int Page = 1, int PageSize = 10, string? Search = null, bool? IsActive = null)
{
    public CategoryFilter() : this(1, 10, null, null)
    {
    }
}

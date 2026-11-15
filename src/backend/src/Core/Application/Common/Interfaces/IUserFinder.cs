using Application.Features.Users.DTOs;

namespace Application.Common.Interfaces;

public interface IUserFinder
{
    Task<IReadOnlyList<UserSummaryResponse>> GetAllAsync(CancellationToken ct = default);
}

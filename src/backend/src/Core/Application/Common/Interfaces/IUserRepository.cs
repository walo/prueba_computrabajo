using Domain.Entities;
using Domain.Primitives;

namespace Application.Common.Interfaces;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}

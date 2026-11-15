using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeding;

public sealed record DatabaseSeedRequest(
    AppDbContext DbContext,
    IPasswordHasher PasswordHasher,
    ILogger Logger);

using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeding;

public sealed record DatabaseSeedRequest(
    AppDbContext DbContext,
    IPasswordHasher PasswordHasher,
    IConfiguration Configuration,
    ILogger Logger);

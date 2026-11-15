using Application.Common.Interfaces;

namespace Infrastructure.Shared.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

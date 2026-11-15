namespace Application.Common.Interfaces;

public interface ITokenBlacklistService
{
    Task BlacklistTokenAsync(BlacklistTokenRequest request, CancellationToken ct = default);
    Task<bool> IsTokenBlacklistedAsync(string jti, CancellationToken ct = default);
}

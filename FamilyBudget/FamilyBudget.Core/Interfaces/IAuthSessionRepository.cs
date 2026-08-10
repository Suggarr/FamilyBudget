using FamilyBudget.Core.Models.Auth;

namespace FamilyBudget.Core.Interfaces;

public interface IAuthSessionRepository
{
    Task<AuthSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthSession>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash,
        CancellationToken cancellationToken = default);

    Task AddAsync(AuthSession session, RefreshToken refreshToken,
        CancellationToken cancellationToken = default);

    Task RotateRefreshTokenAsync(AuthSession session, RefreshToken currentToken,
        RefreshToken replacementToken, CancellationToken cancellationToken = default);

    Task UpdateAsync(AuthSession session, CancellationToken cancellationToken = default);

    Task<bool> IsActiveAsync(Guid sessionId, Guid accountId,
        DateTimeOffset utcNow, CancellationToken cancellationToken = default);
}

using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models.Auth;
using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Persistence.Repositories;

public sealed class AuthSessionRepository : IAuthSessionRepository
{
    private readonly ApplicationDbContext _context;

    public AuthSessionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuthSession?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.AuthSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(session => session.Id == id, cancellationToken);

        return entity is null ? null : MapSession(entity);
    }

    public async Task<IReadOnlyList<AuthSession>> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.AuthSessions
            .AsNoTracking()
            .Where(session => session.AccountId == accountId)
            .OrderByDescending(session => session.LastSeenAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(MapSession).ToList();
    }

    public async Task<RefreshToken?> GetRefreshTokenByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.RefreshTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        return entity is null ? null : MapRefreshToken(entity);
    }

    public async Task AddAsync(
        AuthSession session,
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (session.Id != refreshToken.SessionId)
        {
            throw new InvalidOperationException(
                "Refresh token must belong to the session being created.");
        }

        var sessionEntity = MapSession(session);
        sessionEntity.RefreshTokens.Add(MapRefreshToken(refreshToken));

        await _context.AuthSessions.AddAsync(sessionEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RotateRefreshTokenAsync(
        AuthSession session,
        RefreshToken currentToken,
        RefreshToken replacementToken,
        CancellationToken cancellationToken = default)
    {
        if (session.Id != currentToken.SessionId ||
            session.Id != replacementToken.SessionId)
        {
            throw new InvalidOperationException(
                "Both refresh tokens must belong to the supplied session.");
        }

        if (!currentToken.UsedAtUtc.HasValue ||
            currentToken.ReplacedByTokenId != replacementToken.Id)
        {
            throw new InvalidOperationException(
                "Current refresh token must be marked as used by the replacement token.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var storedToken = await _context.RefreshTokens
                .FromSqlInterpolated(
                    $"SELECT * FROM \"RefreshTokens\" WHERE \"Id\" = {currentToken.Id} FOR UPDATE")
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Refresh token was not found.");

            if (storedToken.UsedAtUtc.HasValue || storedToken.RevokedAtUtc.HasValue)
            {
                throw new InvalidOperationException("Refresh token is no longer active.");
            }

            if (storedToken.ExpiresAtUtc <= currentToken.UsedAtUtc.Value)
            {
                throw new InvalidOperationException("Refresh token has expired.");
            }

            var storedSession = await _context.AuthSessions
                .SingleOrDefaultAsync(entity => entity.Id == session.Id, cancellationToken)
                ?? throw new InvalidOperationException("Authentication session was not found.");

            if (storedSession.RevokedAtUtc.HasValue ||
                storedSession.ExpiresAtUtc <= currentToken.UsedAtUtc.Value)
            {
                throw new InvalidOperationException("Authentication session is not active.");
            }

            var replacementEntity = MapRefreshToken(replacementToken);
            await _context.RefreshTokens.AddAsync(replacementEntity, cancellationToken);

            storedToken.UsedAtUtc = currentToken.UsedAtUtc;
            storedToken.ReplacedByTokenId = replacementToken.Id;
            storedToken.ReplacedByToken = replacementEntity;

            storedSession.LastSeenAtUtc = session.LastSeenAtUtc;
            storedSession.RevokedAtUtc = session.RevokedAtUtc;
            storedSession.RevocationReason = session.RevocationReason;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    public Task UpdateAsync(
        AuthSession session,
        CancellationToken cancellationToken = default) =>
        _context.AuthSessions
            .Where(entity => entity.Id == session.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.LastSeenAtUtc, session.LastSeenAtUtc)
                .SetProperty(entity => entity.RevokedAtUtc, session.RevokedAtUtc)
                .SetProperty(entity => entity.RevocationReason, session.RevocationReason),
                cancellationToken);

    public Task<bool> IsActiveAsync(
        Guid sessionId,
        Guid accountId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken = default) =>
        _context.AuthSessions.AnyAsync(
            session => session.Id == sessionId &&
                       session.AccountId == accountId &&
                       session.RevokedAtUtc == null &&
                       session.ExpiresAtUtc > utcNow &&
                       session.Account.DisabledAtUtc == null,
            cancellationToken);

    private static AuthSession MapSession(AuthSessionEntity entity) =>
        AuthSession.Restore(
            entity.Id,
            entity.AccountId,
            entity.DeviceId,
            entity.DeviceName,
            entity.CreatedAtUtc,
            entity.LastSeenAtUtc,
            entity.ExpiresAtUtc,
            entity.RevokedAtUtc,
            entity.RevocationReason).Value;

    private static AuthSessionEntity MapSession(AuthSession session) => new()
    {
        Id = session.Id,
        AccountId = session.AccountId,
        DeviceId = session.DeviceId,
        DeviceName = session.DeviceName,
        CreatedAtUtc = session.CreatedAtUtc,
        LastSeenAtUtc = session.LastSeenAtUtc,
        ExpiresAtUtc = session.ExpiresAtUtc,
        RevokedAtUtc = session.RevokedAtUtc,
        RevocationReason = session.RevocationReason
    };

    private static RefreshToken MapRefreshToken(RefreshTokenEntity entity) =>
        RefreshToken.Restore(
            entity.Id,
            entity.SessionId,
            entity.TokenHash,
            entity.CreatedAtUtc,
            entity.ExpiresAtUtc,
            entity.UsedAtUtc,
            entity.RevokedAtUtc,
            entity.ReplacedByTokenId).Value;

    private static RefreshTokenEntity MapRefreshToken(RefreshToken token) => new()
    {
        Id = token.Id,
        SessionId = token.SessionId,
        TokenHash = token.TokenHash,
        CreatedAtUtc = token.CreatedAtUtc,
        ExpiresAtUtc = token.ExpiresAtUtc,
        UsedAtUtc = token.UsedAtUtc,
        RevokedAtUtc = token.RevokedAtUtc,
        ReplacedByTokenId = token.ReplacedByTokenId
    };
}

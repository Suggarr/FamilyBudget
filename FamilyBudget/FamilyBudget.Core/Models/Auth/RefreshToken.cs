using CSharpFunctionalExtensions;

namespace FamilyBudget.Core.Models.Auth
{
    public sealed class RefreshToken
    {
        public Guid Id { get; }
        public Guid SessionId { get; }
        public string TokenHash { get; }
        public DateTimeOffset CreatedAtUtc { get; }
        public DateTimeOffset ExpiresAtUtc { get; }
        public DateTimeOffset? UsedAtUtc { get; private set; }
        public DateTimeOffset? RevokedAtUtc { get; private set; }
        public Guid? ReplacedByTokenId { get; private set; }

        private RefreshToken(Guid id, Guid sessionId, string tokenHash,
            DateTimeOffset createdAtUtc, DateTimeOffset expiresAtUtc,
            DateTimeOffset? usedAtUtc, DateTimeOffset? revokedAtUtc, Guid? replacedByTokenId)
        {
            Id = id;
            SessionId = sessionId;
            TokenHash = tokenHash;
            CreatedAtUtc = createdAtUtc;
            ExpiresAtUtc = expiresAtUtc;
            UsedAtUtc = usedAtUtc;
            RevokedAtUtc = revokedAtUtc;
            ReplacedByTokenId = replacedByTokenId;
        }

        public bool IsActive(DateTimeOffset utcNow)
        {
            return ExpiresAtUtc > utcNow && !UsedAtUtc.HasValue && !RevokedAtUtc.HasValue;
        }

        public static Result<RefreshToken> Create(Guid id, Guid sessionId, string tokenHash,
            DateTimeOffset createdAtUtc, DateTimeOffset expiresAtUtc)
        {
            var validationToken = ValidateToken(id, sessionId, tokenHash, createdAtUtc, expiresAtUtc);
            if (validationToken.IsFailure)
            {
                return Result.Failure<RefreshToken>(validationToken.Error);
            }

            var refreshToken = new RefreshToken(id, sessionId, tokenHash.Trim(),
                createdAtUtc, expiresAtUtc, null, null, null);

            return Result.Success<RefreshToken>(refreshToken);
        }

        public static Result<RefreshToken> Restore(Guid id, Guid sessionId, string tokenHash,
            DateTimeOffset createdAtUtc, DateTimeOffset expiresAtUtc,
            DateTimeOffset? usedAtUtc, DateTimeOffset? revokedAtUtc, Guid? replacedByTokenId)
        {
            var validationToken = ValidateToken(id, sessionId, tokenHash, createdAtUtc, expiresAtUtc);
            if (validationToken.IsFailure)
            {
                return Result.Failure<RefreshToken>(validationToken.Error);
            }

            if ((usedAtUtc.HasValue && usedAtUtc.Value.Offset != TimeSpan.Zero) ||
                (revokedAtUtc.HasValue && revokedAtUtc.Value.Offset != TimeSpan.Zero))
            {
                return Result.Failure<RefreshToken>("UsedAtUtc and RevokedAtUtc must be in UTC.");
            }

            if (usedAtUtc.HasValue && usedAtUtc.Value < createdAtUtc)
            {
                return Result.Failure<RefreshToken>("UsedAtUtc cannot be before CreatedAtUtc.");
            }

            if (usedAtUtc.HasValue && usedAtUtc.Value >= expiresAtUtc)
            {
                return Result.Failure<RefreshToken>("UsedAtUtc must be before ExpiresAtUtc.");
            }

            if (revokedAtUtc.HasValue && revokedAtUtc.Value < createdAtUtc)
            {
                return Result.Failure<RefreshToken>("RevokedAtUtc cannot be before CreatedAtUtc.");
            }

            if (replacedByTokenId == id)
            {
                return Result.Failure<RefreshToken>("ReplacedByTokenId cannot be equal to Id.");
            }

            if (replacedByTokenId.HasValue && !usedAtUtc.HasValue)
            {
                return Result.Failure<RefreshToken>("ReplacedByTokenId cannot exist without UsedAtUtc.");
            }

            if (usedAtUtc.HasValue && !replacedByTokenId.HasValue)
            {
                return Result.Failure<RefreshToken>("UsedAtUtc cannot exist without ReplacedByTokenId.");
            }

            var refreshToken = new RefreshToken(id, sessionId, tokenHash.Trim(),
                createdAtUtc, expiresAtUtc, usedAtUtc, revokedAtUtc, replacedByTokenId);

            return Result.Success<RefreshToken>(refreshToken);
        }

        public Result MarkAsUsed(DateTimeOffset usedAtUtc, Guid replacedByTokenId)
        {
            if (usedAtUtc.Offset != TimeSpan.Zero)
            {
                return Result.Failure("UsedAtUtc must be in UTC.");
            }

            if (replacedByTokenId == Guid.Empty)
            {
                return Result.Failure("ReplacedByTokenId cannot be empty.");
            }

            if (replacedByTokenId == Id)
            {
                return Result.Failure("ReplacedByTokenId cannot be equal to Id.");
            }

            if (UsedAtUtc.HasValue)
            {
                return Result.Failure("Refresh token has already been used.");
            }

            if (RevokedAtUtc.HasValue)
            {
                return Result.Failure("Cannot use a revoked refresh token.");
            }

            if (usedAtUtc < CreatedAtUtc)
            {
                return Result.Failure("UsedAtUtc cannot be before CreatedAtUtc.");
            }

            if (ExpiresAtUtc <= usedAtUtc)
            {
                return Result.Failure("Cannot use an expired refresh token.");
            }

            UsedAtUtc = usedAtUtc;
            ReplacedByTokenId = replacedByTokenId;

            return Result.Success();
        }

        public Result Revoke(DateTimeOffset revokedAtUtc)
        {
            if (RevokedAtUtc.HasValue)
            {
                return Result.Success();
            }

            if (revokedAtUtc.Offset != TimeSpan.Zero)
            {
                return Result.Failure("RevokedAtUtc must be in UTC.");
            }

            if (revokedAtUtc < CreatedAtUtc)
            {
                return Result.Failure("RevokedAtUtc cannot be before CreatedAtUtc.");
            }

            RevokedAtUtc = revokedAtUtc;

            return Result.Success();
        }

        private static Result ValidateToken(Guid id, Guid sessionId, string tokenHash,
            DateTimeOffset createdAtUtc, DateTimeOffset expiresAtUtc)
        {
            var normalizedTokenHash = tokenHash?.Trim();

            if (id == Guid.Empty)
            {
                return Result.Failure("Id cannot be empty.");
            }

            if (sessionId == Guid.Empty)
            {
                return Result.Failure("SessionId cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(normalizedTokenHash))
            {
                return Result.Failure("TokenHash cannot be empty.");
            }

            if (createdAtUtc.Offset != TimeSpan.Zero ||
                expiresAtUtc.Offset != TimeSpan.Zero)
            {
                return Result.Failure("CreatedAtUtc and ExpiresAtUtc must be in UTC.");
            }

            if (createdAtUtc >= expiresAtUtc)
            {
                return Result.Failure("CreatedAtUtc must be before ExpiresAtUtc.");
            }

            return Result.Success();
        }
    }
}

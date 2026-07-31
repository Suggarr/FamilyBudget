using CSharpFunctionalExtensions;
using FamilyBudget.Core.Enums;

namespace FamilyBudget.Core.Models.Auth
{
    public sealed class AuthSession
    {
        public const int MaxDeviceIdLength = 100;
        public const int MaxDeviceNameLength = 200;

        public Guid Id { get; }
        public Guid AccountId { get; }
        public string DeviceId { get; }
        public string DeviceName { get; }
        public DateTimeOffset CreatedAtUtc { get; }
        public DateTimeOffset LastSeenAtUtc { get; private set; }
        public DateTimeOffset ExpiresAtUtc { get; }
        public DateTimeOffset? RevokedAtUtc { get; private set; }
        public SessionRevocationReason? RevocationReason { get; private set; }

        private AuthSession(Guid id, Guid accountId, string deviceId, string deviceName, DateTimeOffset createdAtUtc, DateTimeOffset lastSeenAtUtc, 
            DateTimeOffset expiresAtUtc, DateTimeOffset? revokedAtUtc, SessionRevocationReason? revocationReason)
        {
            Id = id;
            AccountId = accountId;
            DeviceId = deviceId;
            DeviceName = deviceName;
            CreatedAtUtc = createdAtUtc;
            LastSeenAtUtc = lastSeenAtUtc;
            ExpiresAtUtc = expiresAtUtc;
            RevokedAtUtc = revokedAtUtc;
            RevocationReason = revocationReason;
        }

        public bool IsActive(DateTimeOffset utcNow)
        {
            return ExpiresAtUtc > utcNow && !RevokedAtUtc.HasValue;
        }

        public static Result<AuthSession> Create(Guid id, Guid accountId, string deviceId, string deviceName, 
            DateTimeOffset createdAtUtc, DateTimeOffset lastSeenAtUtc,
            DateTimeOffset expiresAtUtc, DateTimeOffset? revokedAtUtc, SessionRevocationReason? revocationReason)
        {
            var validationSession = ValidateSession(id, accountId, deviceId, deviceName, createdAtUtc, lastSeenAtUtc, expiresAtUtc, revokedAtUtc);
            if (validationSession.IsFailure)
            {
                return Result.Failure<AuthSession>(validationSession.Error);
            }

            var authSession = new AuthSession(id, accountId, deviceId.Trim(), deviceName.Trim(), createdAtUtc, lastSeenAtUtc, expiresAtUtc, revokedAtUtc, revocationReason);
            return Result.Success(authSession);
        }

        public static Result<AuthSession> Restore(Guid id, Guid accountId, string deviceId, 
            string deviceName, DateTimeOffset createdAtUtc, DateTimeOffset lastSeenAtUtc,
            DateTimeOffset expiresAtUtc, DateTimeOffset? revokedAtUtc, SessionRevocationReason? revocationReason)
        {
            var validationSession = ValidateSession(id, accountId, deviceId, deviceName, createdAtUtc, lastSeenAtUtc, expiresAtUtc, revokedAtUtc);
            if (validationSession.IsFailure)
            {
                return Result.Failure<AuthSession>(validationSession.Error);
            }

            if (lastSeenAtUtc < createdAtUtc)
            {
                return Result.Failure<AuthSession>("LastSeenAtUtc cannot be before CreatedAtUtc.");
            }

            if (lastSeenAtUtc > expiresAtUtc)
            {
                return Result.Failure<AuthSession>("LastSeenAtUtc cannot be after ExpiresAtUtc.");
            }

            if (revokedAtUtc.HasValue && revokedAtUtc.Value < createdAtUtc)
            {
                return Result.Failure<AuthSession>("RevokedAtUtc cannot be before CreatedAtUtc.");
            }

            if (revokedAtUtc.HasValue && !revocationReason.HasValue)
            {
                return Result.Failure<AuthSession>("Revocation reason is required for a revoked session.");
            }

            if (revocationReason.HasValue && !Enum.IsDefined(typeof(SessionRevocationReason), revocationReason.Value))
            {
                return Result.Failure<AuthSession>("Invalid revocation reason.");
            }

            if (!revokedAtUtc.HasValue && revocationReason.HasValue)
            {
                return Result.Failure<AuthSession>("Revocation reason cannot exist without RevokedAtUtc.");
            }

            var authSession = new AuthSession(id, accountId, deviceId.Trim(), deviceName.Trim(), createdAtUtc, lastSeenAtUtc, expiresAtUtc, revokedAtUtc, revocationReason);
            return Result.Success(authSession);
        }

        public Result Touch(DateTimeOffset utcNow)
        {
            if (utcNow.Offset != TimeSpan.Zero)
            {
                return Result.Failure("UtcNow must be in UTC.");
            }
            
            if (RevokedAtUtc.HasValue)
            {
                return Result.Failure("Cannot touch a revoked session.");
            }

            if (ExpiresAtUtc <= utcNow)
            {
                return Result.Failure("Cannot touch an expired session.");
            }

            if (utcNow < LastSeenAtUtc)
            {
                return Result.Failure("UtcNow cannot be before LastSeenAtUtc.");
            }

            LastSeenAtUtc = utcNow;

            return Result.Success();
        }

        public Result Revoke(DateTimeOffset utcNow, SessionRevocationReason revocationReason)
        {
            if (RevokedAtUtc.HasValue)
            {
                return Result.Success();
            }

            if (utcNow.Offset != TimeSpan.Zero)
            {
                return Result.Failure("UtcNow must be in UTC.");
            }

            if (utcNow < CreatedAtUtc)
            {
                return Result.Failure("RevokedAtUtc cannot be before CreatedAtUtc.");
            }

            if (!Enum.IsDefined(typeof(SessionRevocationReason), revocationReason))
            {
                return Result.Failure("Invalid revocation reason.");
            }

            RevokedAtUtc = utcNow;
            RevocationReason = revocationReason;

            return Result.Success();
        }

        private static Result ValidateSession(Guid id, Guid accountId, string deviceId, string deviceName, DateTimeOffset createdAtUtc, DateTimeOffset lastSeenAtUtc,
            DateTimeOffset expiresAtUtc, DateTimeOffset? revokedAtUtc)
        {
            var normalizedDeviceId = deviceId?.Trim();
            var normalizedDeviceName = deviceName?.Trim();

            if (id == Guid.Empty)
            {
                return Result.Failure("Id cannot be empty.");
            }

            if (accountId == Guid.Empty)
            {
                return Result.Failure("AccountId cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(normalizedDeviceId))
            {
                return Result.Failure("DeviceId cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(normalizedDeviceName))
            {
                return Result.Failure("DeviceName cannot be empty.");
            }

            if (normalizedDeviceId.Length > MaxDeviceIdLength)
            {
                return Result.Failure($"DeviceId cannot be longer than {MaxDeviceIdLength} symbols.");
            }

            if (normalizedDeviceName.Length > MaxDeviceNameLength)
            {
                return Result.Failure($"DeviceName cannot be longer than {MaxDeviceNameLength} symbols.");
            }

            if (createdAtUtc.Offset != TimeSpan.Zero || 
                lastSeenAtUtc.Offset != TimeSpan.Zero || 
                expiresAtUtc.Offset != TimeSpan.Zero || 
                (revokedAtUtc.HasValue &&
                revokedAtUtc.Value.Offset != TimeSpan.Zero))
            {
                return Result.Failure("All DateTimeOffset values must be in UTC.");
            }

            if (createdAtUtc >= expiresAtUtc)
            {
                return Result.Failure("CreatedAtUtc must be before ExpiresAtUtc.");
            }

            return Result.Success();
        }
    }   
}

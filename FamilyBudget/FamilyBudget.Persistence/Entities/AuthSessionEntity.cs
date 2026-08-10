using FamilyBudget.Core.Enums;

namespace FamilyBudget.Persistence.Entities;

public sealed class AuthSessionEntity
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public SessionRevocationReason? RevocationReason { get; set; }

    public AccountEntity Account { get; set; } = null!;
    public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = [];
}

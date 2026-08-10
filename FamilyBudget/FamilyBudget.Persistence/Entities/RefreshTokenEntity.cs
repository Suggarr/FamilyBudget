namespace FamilyBudget.Persistence.Entities;

public sealed class RefreshTokenEntity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? UsedAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public Guid? ReplacedByTokenId { get; set; }

    public AuthSessionEntity Session { get; set; } = null!;
    public RefreshTokenEntity? ReplacedByToken { get; set; }
}

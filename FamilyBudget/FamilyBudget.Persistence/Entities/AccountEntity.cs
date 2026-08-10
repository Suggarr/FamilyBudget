namespace FamilyBudget.Persistence.Entities;

public sealed class AccountEntity
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? DisabledAtUtc { get; set; }

    public LocalCredentialEntity? LocalCredential { get; set; }
    public ICollection<AuthSessionEntity> Sessions { get; set; } = [];
}

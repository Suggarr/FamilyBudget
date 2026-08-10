namespace FamilyBudget.Persistence.Entities;

public sealed class LocalCredentialEntity
{
    public Guid AccountId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? PasswordChangedAtUtc { get; set; }

    public AccountEntity Account { get; set; } = null!;
}

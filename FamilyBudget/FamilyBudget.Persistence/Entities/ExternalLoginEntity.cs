using FamilyBudget.Core.Enums;

namespace FamilyBudget.Persistence.Entities;

public sealed class ExternalLoginEntity
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public ExternalLoginProvider Provider { get; set; }
    public string ProviderSubject { get; set; } = string.Empty;
    public string? ProviderUsername { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }

    public AccountEntity Account { get; set; } = null!;
}

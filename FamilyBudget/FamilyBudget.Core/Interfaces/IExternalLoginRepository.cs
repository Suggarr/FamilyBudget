using FamilyBudget.Core.Enums;
using FamilyBudget.Core.Models.Auth;

namespace FamilyBudget.Core.Interfaces;

public interface IExternalLoginRepository
{
    Task<ExternalLogin?> GetAsync(
        ExternalLoginProvider provider,
        string providerSubject,
        CancellationToken cancellationToken = default);

    Task<ExternalLogin?> GetByAccountIdAsync(
        Guid accountId,
        ExternalLoginProvider provider,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExternalLogin>> GetByAccountIdsAsync(
        IReadOnlyCollection<Guid> accountIds,
        ExternalLoginProvider provider,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ExternalLogin externalLogin,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        ExternalLogin externalLogin,
        CancellationToken cancellationToken = default);
}

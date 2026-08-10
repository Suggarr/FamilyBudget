using FamilyBudget.Core.Models.Auth;

namespace FamilyBudget.Core.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LocalCredential?> GetCredentialByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task AddAsync(Account account, LocalCredential credential,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(Account account, CancellationToken cancellationToken = default);

    Task UpdateCredentialAsync(LocalCredential credential, CancellationToken cancellationToken = default);
}

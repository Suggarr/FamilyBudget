using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models.Auth;
using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;

    public AccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Accounts
            .AsNoTracking()
            .SingleOrDefaultAsync(account => account.Id == id, cancellationToken);

        return entity is null ? null : MapAccount(entity);
    }

    public async Task<LocalCredential?> GetCredentialByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.LocalCredentials
            .AsNoTracking()
            .SingleOrDefaultAsync(
                credential => credential.NormalizedEmail == normalizedEmail,
                cancellationToken);

        return entity is null ? null : MapCredential(entity);
    }

    public async Task AddAsync(
        Account account,
        LocalCredential credential,
        CancellationToken cancellationToken = default)
    {
        if (account.Id != credential.AccountId)
        {
            throw new InvalidOperationException(
                "Account and local credential must have the same AccountId.");
        }

        var accountEntity = MapAccount(account);
        accountEntity.LocalCredential = MapCredential(credential);

        await _context.Accounts.AddAsync(accountEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(
        Account account,
        CancellationToken cancellationToken = default) =>
        _context.Accounts
            .Where(entity => entity.Id == account.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.DisplayName, account.DisplayName)
                .SetProperty(entity => entity.DisabledAtUtc, account.DisabledAtUtc),
                cancellationToken);

    public Task UpdateCredentialAsync(
        LocalCredential credential,
        CancellationToken cancellationToken = default) =>
        _context.LocalCredentials
            .Where(entity => entity.AccountId == credential.AccountId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.Email, credential.Email)
                .SetProperty(entity => entity.NormalizedEmail, credential.NormalizedEmail)
                .SetProperty(entity => entity.PasswordHash, credential.PasswordHash)
                .SetProperty(
                    entity => entity.PasswordChangedAtUtc,
                    credential.PasswordChangedAtUtc),
                cancellationToken);

    private static Account MapAccount(AccountEntity entity) =>
        Account.Restore(
            entity.Id,
            entity.DisplayName,
            entity.CreatedAtUtc,
            entity.DisabledAtUtc).Value;

    private static AccountEntity MapAccount(Account account) => new()
    {
        Id = account.Id,
        DisplayName = account.DisplayName,
        CreatedAtUtc = account.CreatedAtUtc,
        DisabledAtUtc = account.DisabledAtUtc
    };

    private static LocalCredential MapCredential(LocalCredentialEntity entity) =>
        LocalCredential.Restore(
            entity.AccountId,
            entity.Email,
            entity.PasswordHash,
            entity.CreatedAtUtc,
            entity.PasswordChangedAtUtc).Value;

    private static LocalCredentialEntity MapCredential(LocalCredential credential) => new()
    {
        AccountId = credential.AccountId,
        Email = credential.Email,
        NormalizedEmail = credential.NormalizedEmail,
        PasswordHash = credential.PasswordHash,
        CreatedAtUtc = credential.CreatedAtUtc,
        PasswordChangedAtUtc = credential.PasswordChangedAtUtc
    };
}

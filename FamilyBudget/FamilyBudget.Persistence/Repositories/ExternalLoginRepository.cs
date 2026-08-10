using FamilyBudget.Core.Enums;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models.Auth;
using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Persistence.Repositories;

public sealed class ExternalLoginRepository : IExternalLoginRepository
{
    private readonly ApplicationDbContext _context;

    public ExternalLoginRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExternalLogin?> GetAsync(
        ExternalLoginProvider provider,
        string providerSubject,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.ExternalLogins
            .AsNoTracking()
            .SingleOrDefaultAsync(
                login => login.Provider == provider &&
                         login.ProviderSubject == providerSubject,
                cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<ExternalLogin?> GetByAccountIdAsync(
        Guid accountId,
        ExternalLoginProvider provider,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.ExternalLogins
            .AsNoTracking()
            .SingleOrDefaultAsync(
                login => login.AccountId == accountId && login.Provider == provider,
                cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<IReadOnlyList<ExternalLogin>> GetByAccountIdsAsync(
        IReadOnlyCollection<Guid> accountIds,
        ExternalLoginProvider provider,
        CancellationToken cancellationToken = default)
    {
        if (accountIds.Count == 0)
        {
            return [];
        }

        var entities = await _context.ExternalLogins
            .AsNoTracking()
            .Where(login => accountIds.Contains(login.AccountId) &&
                            login.Provider == provider)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToList();
    }

    public async Task AddAsync(
        ExternalLogin externalLogin,
        CancellationToken cancellationToken = default)
    {
        await _context.ExternalLogins.AddAsync(Map(externalLogin), cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(
        ExternalLogin externalLogin,
        CancellationToken cancellationToken = default) =>
        _context.ExternalLogins
            .Where(entity => entity.Id == externalLogin.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    entity => entity.ProviderUsername,
                    externalLogin.ProviderUsername)
                .SetProperty(entity => entity.LastSeenAtUtc, externalLogin.LastSeenAtUtc),
                cancellationToken);

    private static ExternalLogin Map(ExternalLoginEntity entity) =>
        ExternalLogin.Restore(
            entity.Id,
            entity.AccountId,
            entity.Provider,
            entity.ProviderSubject,
            entity.ProviderUsername,
            entity.CreatedAtUtc,
            entity.LastSeenAtUtc).Value;

    internal static ExternalLoginEntity Map(ExternalLogin externalLogin) => new()
    {
        Id = externalLogin.Id,
        AccountId = externalLogin.AccountId,
        Provider = externalLogin.Provider,
        ProviderSubject = externalLogin.ProviderSubject,
        ProviderUsername = externalLogin.ProviderUsername,
        CreatedAtUtc = externalLogin.CreatedAtUtc,
        LastSeenAtUtc = externalLogin.LastSeenAtUtc
    };
}

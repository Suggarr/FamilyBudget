using AutoMapper;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Core.Models.Auth;
using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Persistence.Repositories;

public sealed class FamilyMembershipWriter : IFamilyMembershipWriter
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public FamilyMembershipWriter(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task CreateFamilyAsync(
        Family family,
        User user,
        Account? newAccount,
        ExternalLogin externalLogin)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Families.AddAsync(_mapper.Map<FamilyEntity>(family));

            if (newAccount is not null)
            {
                await AddNewAccountAsync(newAccount, user, externalLogin);
            }
            else
            {
                var updatedUsers = await _context.Users
                    .Where(entity => entity.Id == user.Id && entity.FamilyId == null)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(entity => entity.FamilyId, user.FamilyId)
                        .SetProperty(entity => entity.Name, user.Name));

                if (updatedUsers != 1)
                {
                    throw new InvalidOperationException(
                        "User already belongs to a family or no longer exists.");
                }

                await UpdateExternalLoginAsync(externalLogin);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task JoinByInviteAsync(
        FamilyInvite invite,
        User user,
        Account? newAccount,
        ExternalLogin externalLogin)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var consumedInvites = await _context.FamilyInvites
                .Where(entity => entity.Id == invite.Id && entity.ExpiresAt > DateTime.UtcNow)
                .ExecuteDeleteAsync();

            if (consumedInvites != 1)
            {
                throw new InvalidOperationException(
                    "Invite has already been used or expired.");
            }

            if (newAccount is not null)
            {
                await AddNewAccountAsync(newAccount, user, externalLogin);
            }
            else
            {
                var updatedUsers = await _context.Users
                    .Where(entity => entity.Id == user.Id &&
                                     (entity.FamilyId == null ||
                                      entity.FamilyId == user.FamilyId))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(entity => entity.FamilyId, user.FamilyId)
                        .SetProperty(entity => entity.Name, user.Name));

                if (updatedUsers != 1)
                {
                    throw new InvalidOperationException(
                        "User no longer exists or belongs to another family.");
                }

                await UpdateExternalLoginAsync(externalLogin);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task AddNewAccountAsync(
        Account account,
        User user,
        ExternalLogin externalLogin)
    {
        if (account.Id != user.Id || account.Id != externalLogin.AccountId)
        {
            throw new InvalidOperationException(
                "Account, user and external login identifiers must match.");
        }

        var accountEntity = new AccountEntity
        {
            Id = account.Id,
            DisplayName = account.DisplayName,
            CreatedAtUtc = account.CreatedAtUtc,
            DisabledAtUtc = account.DisabledAtUtc,
            User = _mapper.Map<UserEntity>(user)
        };

        accountEntity.ExternalLogins.Add(ExternalLoginRepository.Map(externalLogin));
        await _context.Accounts.AddAsync(accountEntity);
    }

    private async Task UpdateExternalLoginAsync(ExternalLogin externalLogin)
    {
        var updatedLogins = await _context.ExternalLogins
            .Where(entity => entity.Id == externalLogin.Id &&
                             entity.AccountId == externalLogin.AccountId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    entity => entity.ProviderUsername,
                    externalLogin.ProviderUsername)
                .SetProperty(entity => entity.LastSeenAtUtc, externalLogin.LastSeenAtUtc));

        if (updatedLogins != 1)
        {
            throw new InvalidOperationException("External login no longer exists.");
        }
    }
}

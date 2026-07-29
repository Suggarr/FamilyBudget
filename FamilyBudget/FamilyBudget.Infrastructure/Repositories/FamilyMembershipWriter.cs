using AutoMapper;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Infrastructure.Repositories;

public class FamilyMembershipWriter : IFamilyMembershipWriter
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public FamilyMembershipWriter(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task CreateFamilyAsync(Family family, User user, bool isNewUser)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Families.AddAsync(_mapper.Map<FamilyEntity>(family));
            await _context.SaveChangesAsync();

            if (isNewUser)
            {
                await _context.Users.AddAsync(_mapper.Map<UserEntity>(user));
            }
            else
            {
                var updatedUsers = await _context.Users
                    .Where(u => u.Id == user.Id && u.FamilyId == null)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(u => u.FamilyId, user.FamilyId)
                        .SetProperty(u => u.Name, user.Name)
                        .SetProperty(u => u.TelegramUsername, user.TelegramUsername));

                if (updatedUsers != 1)
                    throw new InvalidOperationException("User already belongs to a family or no longer exists.");
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

    public async Task JoinByInviteAsync(FamilyInvite invite, User user, bool isNewUser)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var consumedInvites = await _context.FamilyInvites
                .Where(i => i.Id == invite.Id && i.ExpiresAt > DateTime.UtcNow)
                .ExecuteDeleteAsync();

            if (consumedInvites != 1)
                throw new InvalidOperationException("Invite has already been used or expired.");

            if (isNewUser)
            {
                await _context.Users.AddAsync(_mapper.Map<UserEntity>(user));
            }
            else
            {
                var updatedUsers = await _context.Users
                    .Where(u => u.Id == user.Id)
                    .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.FamilyId, user.FamilyId)
                    .SetProperty(u => u.Name, user.Name)
                    .SetProperty(u => u.TelegramUsername, user.TelegramUsername));

                if (updatedUsers != 1)
                    throw new InvalidOperationException("User no longer exists.");
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
}

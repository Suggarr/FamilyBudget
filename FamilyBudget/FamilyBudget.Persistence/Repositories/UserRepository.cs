using AutoMapper;
using FamilyBudget.Core.Enums;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UserRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<User?> GetByExternalLoginAsync(
        ExternalLoginProvider provider,
        string providerSubject)
    {
        var userEntity = await _context.Users
            .AsNoTracking()
            .Where(user => user.Account.DisabledAtUtc == null)
            .Where(user => user.Account.ExternalLogins.Any(login =>
                login.Provider == provider &&
                login.ProviderSubject == providerSubject))
            .SingleOrDefaultAsync();

        return userEntity is null ? null : _mapper.Map<User>(userEntity);
    }

    public async Task<List<User>> GetByFamilyIdAsync(Guid familyId)
    {
        var userEntities = await _context.Users
            .AsNoTracking()
            .Where(user => user.FamilyId == familyId)
            .ToListAsync();

        return _mapper.Map<List<User>>(userEntities);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .Where(user => user.Account.DisabledAtUtc == null)
            .Where(user => user.Id == id)
            .SingleOrDefaultAsync();

        return entity is null ? null : _mapper.Map<User>(entity);
    }

    public Task UpdateAsync(User user) =>
        _context.Users
            .Where(entity => entity.Id == user.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.Name, user.Name)
                .SetProperty(entity => entity.FamilyId, user.FamilyId)
                .SetProperty(entity => entity.Balance, user.Balance));
}

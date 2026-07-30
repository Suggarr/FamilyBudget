using AutoMapper;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Persistence.Repositories;

public class SavingsWithdrawalRepository : ISavingsWithdrawalRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SavingsWithdrawalRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<SavingsWithdrawal>> GetByFamilyIdAsync(Guid familyId)
    {
        var entities = await _context.SavingsWithdrawals
            .AsNoTracking()
            .Where(w => w.FamilyId == familyId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<SavingsWithdrawal>>(entities);
    }
}

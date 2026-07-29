using AutoMapper;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Infrastructure.Repositories;

public class SavingsContributionRepository : ISavingsContributionRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SavingsContributionRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<SavingsContribution>> GetByFamilyIdAsync(Guid familyId)
    {
        var entities = await _context.SavingsContributions
            .AsNoTracking()
            .Where(c => c.FamilyId == familyId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<SavingsContribution>>(entities);
    }
}

using AutoMapper;
using CSharpFunctionalExtensions;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Repositories
{
    public class GoalRepository : IGoalRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GoalRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Goal>> GetByFamilyIdAsync(Guid familyId)
        {
            var goalEntities = await _context.Goals
                .AsNoTracking()
                .Where(g => g.FamilyId == familyId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<Goal>>(goalEntities);
        }

        public async Task<Guid> AddAsync(Goal goal)
        {
            var goalEntity = _mapper.Map<GoalEntity>(goal); ;

            _context.Goals.Add(goalEntity);
            await _context.SaveChangesAsync();

            return goal.Id;

        }

        public async Task<Goal?> GetByIdAsync(Guid id)
        {
            var goalEntity = await _context.Goals
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);

            return goalEntity is null ? null : _mapper.Map<Goal>(goalEntity);
        }

        public async Task UpdateAsync(Goal goal)
        {
            _context.Goals.Update(_mapper.Map<GoalEntity>(goal));
            await _context.SaveChangesAsync();
        }

        public async Task<Guid> DeleteAsync(Guid id)
        {
            var goalEntity = await _context.Goals.FindAsync(id);
            if (goalEntity != null)
            {
                _context.Goals.Remove(goalEntity);
                await _context.SaveChangesAsync();
            }
            return id;
        }
    }
}

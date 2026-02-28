using CSharpFunctionalExtensions;
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
    public class GoalRepository 
    {
        private readonly ApplicationDbContext _context;

        public GoalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Goal>> GetByFamilyId(Guid familyId)
        {
            var goalEntities = await _context.Goals
                .AsNoTracking()
                .Where(g => g.FamilyId == familyId)
                .ToListAsync();

            return goalEntities.Select(g => Goal.Create(g.Id, g.FamilyId, g.Title, g.TargetAmount, g.CurrentAmount).Value);
        }

        public async Task<Guid> AddAsync(Goal goal)
        {
            var goalEntity = new GoalEntity
            {
                Id = goal.Id,
                FamilyId = goal.FamilyId,
                Title = goal.Title,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = goal.CurrentAmount
            };

            _context.Goals.Add(goalEntity);
            await _context.SaveChangesAsync();

            return goal.Id;

        }
        public async Task<Guid> UpdateAsync(Guid id, string title, decimal targetAmount, decimal currentAmount)
        {
            await _context.Goals
                .Where(g => g.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(g => g.Title, title)
                    .SetProperty(g => g.TargetAmount, targetAmount)
                    .SetProperty(g => g.CurrentAmount, currentAmount));

            return id;
        }
    }
}

using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Repositories
{
    public class GoalRepository : Repository<Goal>
    {
        public GoalRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Goal>> GetByFamilyId(Guid familyId)
        {
            return await _dbSet.Where(g => g.FamilyId == familyId).ToListAsync();
        }

    }
}

using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Repositories
{
    public class IncomeRepository : Repository<Income>
    {
        public IncomeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Income>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet.Where(i => i.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<Income>> GetByFamilyIdAsync(Guid familyId)
        {
            return await _dbSet.Where(e => e.FamilyId == familyId).ToListAsync();
        }
    }
}

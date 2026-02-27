using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Repositories
{
    public class ExpenseRepository : Repository<Expense>
    {
        public ExpenseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Expense>> GetByFamilyIdAsync(Guid familyId)
        {
            return await _dbSet.Where(e => e.FamilyId == familyId).ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet.Where(e => e.UserId == userId).ToListAsync();
        }
    }
}

using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Repositories
{
    public class ReceiptRepository : Repository<Receipt>
    {
        public ReceiptRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Receipt>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet.Where(r => r.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<Receipt>> GetByFamilyIdAsync(Guid familyId)
        {
            return await _dbSet.Where(r => r.FamilyId == familyId).ToListAsync();
        }

        public async Task<IEnumerable<Receipt>> GetUnprocessedReceiptsAsync()
        {
            return await _dbSet.Where(r => !r.IsProcessed).ToListAsync();
        }
    }
}

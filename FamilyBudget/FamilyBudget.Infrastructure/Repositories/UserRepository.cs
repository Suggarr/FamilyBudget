using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByTelegramIdAsync(long telegramId)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        }

        public async Task<IEnumerable<User>> GetByFamilyIdAsync(Guid familyId)
        {
            return await _dbSet.Where(u => u.FamilyId == familyId).ToListAsync();
        }
    }
}

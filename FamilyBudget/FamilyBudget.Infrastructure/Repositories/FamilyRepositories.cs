using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Repositories
{
    public class FamilyRepositories : Repository<Family>
    {
        public FamilyRepositories(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Family?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(f => f.Name == name);
        }
    }
}

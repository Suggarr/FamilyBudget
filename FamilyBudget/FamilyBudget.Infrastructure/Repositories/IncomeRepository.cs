using FamilyBudget.Core.Models;
using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FamilyBudget.Infrastructure.Repositories
{
    public class IncomeRepository
    {
        private readonly ApplicationDbContext _context;

        public IncomeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Income>> GetByFamilyAsync(Guid familyId)
        {
            var incomeEntities = await _context.Incomes
                .AsNoTracking()
                .Where(i => i.FamilyId == familyId)
                .ToListAsync();

            return incomeEntities.Select(i => Income.Create(i.Id, i.FamilyId, i.UserId, i.Amount, i.Source, i.Date).Value)
                .ToList();
        }

        public async Task<List<Income>> GetByUserIdAsync(Guid userId)
        {
            var incomeEntities = await _context.Incomes
                .AsNoTracking()
                .Where(i => i.UserId == userId)
                .ToListAsync();

            return incomeEntities.Select(i => Income.Create(i.Id, i.FamilyId, i.UserId, i.Amount, i.Source, i.Date).Value)
                .ToList();
        }

        public async Task<IEnumerable<Income>> GetByFamilyIdAsync(Guid familyId)
        {
            var incomeEntities = await _context.Incomes
                .AsNoTracking()
                .Where(i => i.FamilyId == familyId)
                .ToListAsync();

            return incomeEntities.Select(i => Income.Create(i.Id, i.FamilyId, i.UserId, i.Amount, i.Source, i.Date).Value)
                .ToList();
        }

        public async Task<Guid> AddAsync(Income income)
        {
            var incomeEntity = new IncomeEntity
            {
                Id = income.Id,
                FamilyId = income.FamilyId,
                UserId = income.UserId,
                Amount = income.Amount,
                Source = income.Source,
                Date = income.Date
            }
            ;
            await _context.Incomes.AddAsync(incomeEntity);
            await _context.SaveChangesAsync();

            return income.Id;
        }

        public async Task<Guid> UpdateAsync(Guid id, decimal amount, string source, DateTime date)
        {
            await _context.Incomes
                .Where(i => i.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(i => i.Amount, amount)
                    .SetProperty(i => i.Source, source)
                    .SetProperty(i => i.Date, date));

            return id;
        }
    }
}

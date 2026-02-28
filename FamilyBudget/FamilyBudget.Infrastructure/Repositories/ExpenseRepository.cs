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
    public class ExpenseRepository 
    {
        private readonly ApplicationDbContext _context;

        public ExpenseRepository(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task<List<Expense>> GetByFamilyIdAsync(Guid familyId)
        {
            var expenseEntities = await _context.Expenses
                .AsNoTracking()
                .Where(e => e.FamilyId == familyId)
                .ToListAsync();

            return expenseEntities.Select(e => Expense.Create(e.Id, e.FamilyId, e.UserId, e.CategoryId, e.Amount,
                e.Description, e.Date).Value).ToList();
        }

        public async Task<List<Expense>> GetByUserIdAsync(Guid userId)
        {
            var expenseEntities = await _context.Expenses
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .ToListAsync();

            return expenseEntities.Select(e => Expense.Create(e.Id, e.FamilyId, e.UserId, e.CategoryId, e.Amount,
                e.Description, e.Date).Value).ToList();
        }

        public async Task<Guid> AddAsync(Expense expense)
        {
            var expenseEntity = new ExpenseEntity
            {
                Id = expense.Id,
                FamilyId = expense.FamilyId,
                UserId = expense.UserId,
                CategoryId = expense.CategoryId,
                Amount = expense.Amount,
                Description = expense.Description,
                Date = expense.Date

            };

            await _context.Expenses.AddAsync(expenseEntity);
            await _context.SaveChangesAsync();

            return expense.Id;
        }

        public async Task<Guid> DeleteAsync(Guid id)
        {
            var entity = await _context.Expenses.FindAsync(id);
            if (entity != null)
            {
                _context.Expenses.Remove(entity);
                await _context.SaveChangesAsync();
            }

            return id;
        }


        public async Task<Guid> UpdateAsync(Guid id, decimal amount, string description, DateTime date, Guid categoryId)
        {
            await _context.Expenses
                .Where(e => e.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.Amount, amount)
                    .SetProperty(e => e.Description, description)
                    .SetProperty(e => e.Date, date)
                    .SetProperty(e => e.CategoryId, categoryId));

            return id;
        }
    }
}

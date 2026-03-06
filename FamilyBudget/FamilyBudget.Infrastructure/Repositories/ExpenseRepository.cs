using AutoMapper;
using CSharpFunctionalExtensions;
using FamilyBudget.Core.Enums;
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
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ExpenseRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<Expense>> GetByFamilyIdAsync(Guid familyId)
        {
            var expenseEntities = await _context.Expenses
                .AsNoTracking()
                .Where(e => e.FamilyId == familyId)
                .ToListAsync();

            return _mapper.Map<List<Expense>>(expenseEntities);
        }

        public async Task<List<Expense>> GetByUserIdAsync(Guid userId)
        {
            var expenseEntities = await _context.Expenses
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .ToListAsync();

            return _mapper.Map<List<Expense>>(expenseEntities);
        }

        public async Task<List<Expense>> GetByPeriodAsync(Guid familyId, DateTime startDate, DateTime endDate)
        {
            var expenseEntities = await _context.Expenses
                .AsNoTracking()
                .Where(e => e.FamilyId == familyId
                    && e.Date >= startDate
                    && e.Date < endDate)
                .ToListAsync();

            return _mapper.Map<List<Expense>>(expenseEntities);
        }

        public async Task<Guid> AddAsync(Expense expense)
        {
            var expenseEntity = _mapper.Map<ExpenseEntity>(expense);

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

        public async Task<Guid> UpdateAsync(Guid id, decimal amount, string description, DateTime date, ExpenseCategory category)
        {
            await _context.Expenses
                .Where(e => e.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.Amount, amount)
                    .SetProperty(e => e.Description, description)
                    .SetProperty(e => e.Date, date)
                    .SetProperty(e => e.Category, category));

            return id;
        }
    }
}

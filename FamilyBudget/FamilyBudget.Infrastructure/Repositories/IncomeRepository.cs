using AutoMapper;
using FamilyBudget.Core.Interfaces;
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
    public class IncomeRepository : IIncomeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public IncomeRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<Income>> GetByFamilyAsync(Guid familyId)
        {
            var incomeEntities = await _context.Incomes
                .AsNoTracking()
                .Where(i => i.FamilyId == familyId)
                .ToListAsync();

            return _mapper.Map<List<Income>>(incomeEntities);
        }

        public async Task<List<Income>> GetByUserIdAsync(Guid userId)
        {
            var incomeEntities = await _context.Incomes
                .AsNoTracking()
                .Where(i => i.UserId == userId)
                .ToListAsync();

            return _mapper.Map<List<Income>>(incomeEntities);
        }

        public async Task<List<Income>> GetByFamilyIdAsync(Guid familyId)
        {
            var incomeEntities = await _context.Incomes
                .AsNoTracking()
                .Where(i => i.FamilyId == familyId)
                .ToListAsync();

            return _mapper.Map<List<Income>>(incomeEntities);
        }

        public async Task<List<Income>> GetByPeriodAsync(Guid familyId, DateTime startDate, DateTime endDate)
        {
            var incomeEntities = await _context.Incomes
                .AsNoTracking()
                .Where(e => e.FamilyId == familyId
                    && e.Date >= startDate
                    && e.Date < endDate)
                .ToListAsync();

            return _mapper.Map<List<Income>>(incomeEntities);
        }

        public async Task<Guid> AddAsync(Income income)
        {
            var incomeEntity = _mapper.Map<IncomeEntity>(income);

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

        public async Task<Guid> DeleteAsync(Guid id)
        {
            var entity = await _context.Incomes.FindAsync(id);
            if (entity != null)
            {
                _context.Incomes.Remove(entity);
                await _context.SaveChangesAsync();
            }

            return id;
        }
    }
}

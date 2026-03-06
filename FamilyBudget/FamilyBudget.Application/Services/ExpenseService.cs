using AutoMapper;
using FamilyBudget.Core.Dtos.Expense;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IMapper _mapper;

        public ExpenseService(IExpenseRepository expenseRepository, IMapper mapper)
        {
            _expenseRepository = expenseRepository;
            _mapper = mapper;
        }

        public async Task<Guid> AddAsync(CreateExpenseDto dto)
        {
            var expenseResult = Expense.Create(
                Guid.NewGuid(),
                dto.FamilyId,
                dto.UserId,
                dto.CategoryId,
                dto.Amount,
                dto.Description,
                dto.Date);

            if (expenseResult.IsFailure)
            {
                throw new Exception(expenseResult.Error);
            }

            var expense = expenseResult.Value;

            return await _expenseRepository.AddAsync(expense);
        }

        public async Task<List<ExpenseDto>> GetByFamilyIdAsync(Guid familyId)
        {
            var expenses = await _expenseRepository.GetByFamilyIdAsync(familyId);
            return _mapper.Map<List<ExpenseDto>>(expenses);
        }

        public async Task<List<ExpenseDto>> GetByPeriodAsync(Guid familyId, DateTime startDate, DateTime endDate)
        {
            var expenses = await _expenseRepository.GetByPeriodAsync(familyId, startDate, endDate);
            return _mapper.Map<List<ExpenseDto>>(expenses);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _expenseRepository.DeleteAsync(id);
        }
    }
}

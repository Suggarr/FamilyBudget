using FamilyBudget.Application.Dtos;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Services
{
    public class AnaliticsService
    {
        private readonly IIncomeRepository _incomeRepository;
        private readonly IExpenseRepository _expenseRepository;

        public AnaliticsService(IIncomeRepository incomeRepository, IExpenseRepository expenseRepository)
        {
            _incomeRepository = incomeRepository;
            _expenseRepository = expenseRepository;
        }

        public async Task<BalanceDto> GetUserBalanceAsync(Guid userId)
        {
            var incomes = await _incomeRepository.GetByUserIdAsync(userId);
            var expenses = await _expenseRepository.GetByUserIdAsync(userId);

            var totalIncome = incomes.Sum(i => i.Amount);
            var totalExpense = expenses.Sum(e => e.Amount);

            return new BalanceDto(totalIncome, totalExpense, totalIncome - totalExpense);
        }

        public async Task<BalanceDto> GetFamilyBalanceAsync(Guid familyId)
        {
            var incomes = await _incomeRepository.GetByFamilyIdAsync(familyId);
            var expenses = await _expenseRepository.GetByFamilyIdAsync(familyId);

            var totalIncome = incomes.Sum(i => i.Amount);
            var totalExpense = expenses.Sum(e => e.Amount);

            return new BalanceDto(totalIncome, totalExpense, totalIncome - totalExpense);
        }
    }
}
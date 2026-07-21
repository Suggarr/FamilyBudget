using FamilyBudget.Application.Dtos;
using FamilyBudget.Core.Interfaces;
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
        private readonly IUserRepository _userRepository;

        public AnaliticsService(IIncomeRepository incomeRepository, IExpenseRepository expenseRepository, IUserRepository userRepository)
        {
            _incomeRepository = incomeRepository;
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
        }

        public async Task<BalanceDto> GetUserBalanceAsync(Guid userId)
        {
            var incomes = await _incomeRepository.GetByUserIdAsync(userId);
            var expenses = await _expenseRepository.GetByUserIdAsync(userId);

            var totalIncome = incomes.Sum(i => i.Amount);
            var totalExpense = expenses.Sum(e => e.Amount);

            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            return new BalanceDto(totalIncome, totalExpense, user.Balance);
        }

        public async Task<BalanceDto> GetFamilyBalanceAsync(Guid familyId)
        {
            var incomes = await _incomeRepository.GetByFamilyIdAsync(familyId);
            var expenses = await _expenseRepository.GetByFamilyIdAsync(familyId);

            var totalIncome = incomes.Sum(i => i.Amount);
            var totalExpense = expenses.Sum(e => e.Amount);

            var users = await _userRepository.GetByFamilyIdAsync(familyId);
            return new BalanceDto(totalIncome, totalExpense, users.Sum(u => u.Balance));
        }
    }
}

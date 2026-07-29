using AutoMapper;
using FamilyBudget.Core.Dtos.Expense;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
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
        private readonly IUserRepository _userRepository;
        private readonly IFinanceWriter _financeWriter;
        private readonly IMapper _mapper;

        public ExpenseService(IExpenseRepository expenseRepository, IUserRepository userRepository, IFinanceWriter financeWriter, IMapper mapper)
        {
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
            _financeWriter = financeWriter;
            _mapper = mapper;
        }

        public async Task<Guid> AddAsync(CreateExpenseDto dto)
        {
            var expenseResult = Expense.Create(
                Guid.NewGuid(),
                dto.FamilyId,
                dto.UserId,
                dto.Category,
                dto.Amount,
                dto.Description,
                dto.Date);

            if (expenseResult.IsFailure)
            {
                throw new Exception(expenseResult.Error);
            }

            var expense = expenseResult.Value;
            var user = await _userRepository.GetByIdAsync(dto.UserId)
                ?? throw new InvalidOperationException("User not found.");

            if (user.FamilyId != dto.FamilyId)
                throw new InvalidOperationException("User does not belong to this family.");

            var debitResult = user.Debit(dto.Amount);
            if (debitResult.IsFailure)
                throw new InvalidOperationException(debitResult.Error);

            await _financeWriter.AddExpenseAsync(expense, user);
            return expense.Id;
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
            var expense = await _expenseRepository.GetByIdAsync(id);
            if (expense is null)
                return;

            var user = await _userRepository.GetByIdAsync(expense.UserId)
                ?? throw new InvalidOperationException("User not found.");
            var creditResult = user.Credit(expense.Amount);
            if (creditResult.IsFailure)
                throw new InvalidOperationException(creditResult.Error);

            await _financeWriter.DeleteExpenseAsync(expense, user);
        }
    }
}

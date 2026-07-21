using AutoMapper;
using FamilyBudget.Application.Dtos.Income;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Services
{
    public class IncomeService : IIncomeService
    {
        private readonly IIncomeRepository _incomeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFinanceWriter _financeWriter;
        private readonly IMapper _mapper;

        public IncomeService(IIncomeRepository incomeRepository, IUserRepository userRepository, IFinanceWriter financeWriter, IMapper mapper)
        {
            _incomeRepository = incomeRepository;
            _userRepository = userRepository;
            _financeWriter = financeWriter;
            _mapper = mapper;
        }

        public async Task<Guid> AddAsync(CreateIncomeDto dto)
        {
            var incomeResult = Income.Create(
                Guid.NewGuid(),
                dto.FamilyId,
                dto.UserId,
                dto.Amount,
                dto.Description,
                dto.Date);

            if (incomeResult.IsFailure)
            {
                throw new Exception(incomeResult.Error);
            }

            var income = incomeResult.Value;
            var user = await _userRepository.GetByIdAsync(dto.UserId)
                ?? throw new InvalidOperationException("User not found.");

            if (user.FamilyId != dto.FamilyId)
                throw new InvalidOperationException("User does not belong to this family.");

            var creditResult = user.Credit(dto.Amount);
            if (creditResult.IsFailure)
                throw new InvalidOperationException(creditResult.Error);

            await _financeWriter.AddIncomeAsync(income, user);
            return income.Id;
        }

        public async Task<List<IncomeDto>> GetByFamilyAsync(Guid familyId)
        {
            var incomes = await _incomeRepository.GetByFamilyAsync(familyId);
            return _mapper.Map<List<IncomeDto>>(incomes);
        }

        public async Task<List<IncomeDto>> GetByPeriodAsync(
            Guid familyId,
            DateTime from,
            DateTime to)
        {
            var incomes = await _incomeRepository
                .GetByPeriodAsync(familyId, from, to);

            return _mapper.Map<List<IncomeDto>>(incomes);
        }

        public async Task DeleteAsync(Guid id)
        {
            var income = await _incomeRepository.GetByIdAsync(id);
            if (income is null)
                return;

            var user = await _userRepository.GetByIdAsync(income.UserId)
                ?? throw new InvalidOperationException("User not found.");
            var debitResult = user.Debit(income.Amount);
            if (debitResult.IsFailure)
                throw new InvalidOperationException("Income cannot be deleted because the account no longer has enough funds.");

            await _financeWriter.DeleteIncomeAsync(income, user);
        }
    }
}

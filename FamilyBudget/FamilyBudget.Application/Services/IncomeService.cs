using AutoMapper;
using FamilyBudget.Application.Dtos.Income;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Services
{
    public class IncomeService
    {
        private readonly IIncomeRepository _incomeRepository;
        private readonly IMapper _mapper;

        public IncomeService(IIncomeRepository incomeRepository, IMapper mapper)
        {
            _incomeRepository = incomeRepository;
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
            return await _incomeRepository.AddAsync(income);
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
            await _incomeRepository.DeleteAsync(id);
        }
    }
}

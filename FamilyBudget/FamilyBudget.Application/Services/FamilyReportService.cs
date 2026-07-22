using FamilyBudget.Application.Dtos;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;

namespace FamilyBudget.Application.Services
{
    public class FamilyReportService : IFamilyReportService
    {
        private readonly IExpenseRepository _expenseRepository;    
        private readonly IIncomeRepository _incomeRepository;

        public FamilyReportService(IExpenseRepository expenseRepository,
            IIncomeRepository incomeRepository)
        {
            _expenseRepository = expenseRepository;
            _incomeRepository = incomeRepository;
        }

        public async Task<FamilyReportDto> GetFamilyReportAsync(Guid familyId, DateTime? startDate = null,
            DateTime? endDate = null)
        {
            List<Income> incomes;
            List<Expense> expenses;
            if (startDate.HasValue != endDate.HasValue)
            {
                throw new ArgumentException("There's no start date or end date");
            }

            if (startDate.HasValue && startDate.Value >= endDate!.Value)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            if (startDate != null && endDate != null)
            {
                incomes = await _incomeRepository.GetByPeriodAsync(familyId, startDate.Value, endDate.Value);
                expenses = await _expenseRepository.GetByPeriodAsync(familyId, startDate.Value, endDate.Value);
            }
            else
            {
                incomes = await _incomeRepository.GetByFamilyAsync(familyId);
                expenses = await _expenseRepository.GetByFamilyIdAsync(familyId);
            }

            var totalIncome = incomes.Sum(i => i.Amount);
            var totalExpenses = expenses.Sum(e => e.Amount);
            
            var netAmount = totalIncome - totalExpenses;

            var expensePercent = totalIncome == 0 ? 0 : totalExpenses / totalIncome * 100;
            var topCategoryInfo = expenses
                .GroupBy(e => e.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(e => e.Amount),
                })
                .OrderByDescending(g => g.Amount)
                .FirstOrDefault();

            return new FamilyReportDto
            (
                startDate,
                endDate,
                totalIncome,
                totalExpenses,
                netAmount,
                expensePercent,
                topCategoryInfo?.Category.ToString(),
                topCategoryInfo?.Amount
            );
        }
    }
}

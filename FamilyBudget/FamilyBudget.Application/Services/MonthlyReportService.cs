using FamilyBudget.Application.Dtos;
using FamilyBudget.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Services
{
    public class MonthlyReportService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IIncomeRepository _incomeRepository;

        public MonthlyReportService(IExpenseRepository expenseRepository, IIncomeRepository incomeRepository)
        {
            _expenseRepository = expenseRepository;
            _incomeRepository = incomeRepository;
        }

        public async Task<MonthlyReportDto> GetFamilyMonthlyReportAsync(Guid familyId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);

            var incomes = await _incomeRepository.GetByPeriodAsync(familyId, startDate, endDate);

            var expenses = await _expenseRepository.GetByPeriodAsync(familyId, startDate, endDate); 

            var totalIncome = incomes.Sum(i => i.Amount);
            var totalExpenses = expenses.Sum(e => e.Amount);
            var balance = totalIncome - totalExpenses;

            decimal percent = totalIncome > 0 ? (totalExpenses / totalIncome) * 100 : 0;

            var grouped = expenses
                .GroupBy(e => e.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.Amount)
                .FirstOrDefault();

            string? topCategoryName = grouped?.Category.ToString();
            decimal? topCategoryAmount = grouped?.Amount;

            return new MonthlyReportDto(
                totalIncome,
                totalExpenses,
                balance,
                percent,
                topCategoryName,
                topCategoryAmount);
        }
    }
}

using FamilyBudget.Application.Dtos;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Infrastructure.Repositories;
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
        private readonly ICategoryRepository _categoryRepository;

        public MonthlyReportService(IExpenseRepository expenseRepository, IIncomeRepository incomeRepository, ICategoryRepository categoryRepository)
        {
            _expenseRepository = expenseRepository;
            _incomeRepository = incomeRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<MonthlyReportDto> GetFamilyMonthlyReportAsync(Guid familyId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var incomes = await _incomeRepository.GetByPeriodAsync(familyId, startDate, endDate);

            var expenses = await _expenseRepository.GetByPeriodAsync(familyId, startDate, endDate); 

            var totalIncome = incomes.Sum(i => i.Amount);
            var totalExpenses = expenses.Sum(e => e.Amount);
            var balance = totalIncome - totalExpenses;

            decimal percent = totalIncome > 0 ? (totalExpenses / totalIncome) * 100 : 0;

            var grouped = expenses
                .GroupBy(e => e.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Amount = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.Amount)
                .FirstOrDefault();

            string? topCategoryName = null;
            decimal? topCategoryAmount = null;

            if (grouped != null)
            {
                var category = await _categoryRepository
                    .GetByIdAsync(grouped.CategoryId);

                topCategoryName = category?.Name;
                topCategoryAmount = grouped.Amount;
            }

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

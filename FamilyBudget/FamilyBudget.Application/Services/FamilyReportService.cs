using FamilyBudget.Application.Dtos;
using FamilyBudget.Application.Dtos.Savings;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;

namespace FamilyBudget.Application.Services
{
    public class FamilyReportService : IFamilyReportService
    {
        private readonly IExpenseRepository _expenseRepository;    
        private readonly IIncomeRepository _incomeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISavingsContributionRepository _savingsContributionRepository;
        private readonly ISavingsWithdrawalRepository _savingsWithdrawalRepository;

        public FamilyReportService(IExpenseRepository expenseRepository, IIncomeRepository incomeRepository, IUserRepository userRepository,
            ISavingsContributionRepository savingsContributionRepository, ISavingsWithdrawalRepository savingsWithdrawalRepository)
        {
            _expenseRepository = expenseRepository;
            _incomeRepository = incomeRepository;
            _userRepository = userRepository;
            _savingsContributionRepository = savingsContributionRepository;
            _savingsWithdrawalRepository = savingsWithdrawalRepository;
        }

        public async Task<FamilyReportDto> GetFamilyReportAsync(Guid familyId, DateTime? startDate = null,
            DateTime? endDate = null)
        {
            List<Income> incomes;
            List<Expense> expenses;
            List<CategoryStatisticDto> expenseCategories ;
            var memberStatistic = new List<MemberStatisticDto>();

            decimal totalExpenses = 0;

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

            var savingsContribution = await _savingsContributionRepository.GetByFamilyIdAsync(familyId);
            var savingsWithdrawal = await _savingsWithdrawalRepository.GetByFamilyIdAsync(familyId);

            var periodContribution = savingsContribution
                .Where(sc => (!startDate.HasValue || sc.CreatedAt >= startDate.Value) && (!endDate.HasValue || sc.CreatedAt < endDate.Value))
                .Sum(sc => sc.Amount);

            var periodWithdrawal = savingsWithdrawal
                .Where(sw => (!startDate.HasValue || sw.CreatedAt >= startDate.Value) && (!endDate.HasValue || sw.CreatedAt < endDate.Value))
                .Sum(sw => sw.Amount);

            var periodNetChange = periodContribution - periodWithdrawal;
            var currentBalance = savingsContribution.Sum(sc => sc.Amount) - savingsWithdrawal.Sum(sw => sw.Amount);

            totalExpenses = expenses.Sum(e => e.Amount);
            expenseCategories = expenses
                .GroupBy(e => e.Category)
                .Select(g => new { g.Key, Sum = g.Sum(e => e.Amount)})
                .Select(x => new CategoryStatisticDto(
                    x.Key,
                    x.Sum,
                    totalExpenses == 0 ? 0 : x.Sum / totalExpenses * 100
                ))
                .OrderByDescending(x => x.Amount)
                .ToList();

            var totalIncome = incomes.Sum(i => i.Amount);

            var netAmount = totalIncome - totalExpenses;

            var expensePercent = totalIncome == 0 ? 0 : totalExpenses / totalIncome * 100;
            var topCategoryInfo = expenseCategories.FirstOrDefault();

            var users = await _userRepository.GetByFamilyIdAsync(familyId);
            memberStatistic.AddRange(users.Select(u => new
            {
                MemberName = u.Name,
                incomesUser = incomes.Where(i => i.UserId == u.Id).Sum(i => i.Amount),
                expensesUser = expenses.Where(e => e.UserId == u.Id).Sum(e => e.Amount),
            })
            .Select(x => new MemberStatisticDto
            (
                x.MemberName,
                x.incomesUser,
                x.expensesUser,
                x.incomesUser - x.expensesUser
            )));

            return new FamilyReportDto
            (
                startDate,
                endDate,
                totalIncome,
                totalExpenses,
                netAmount,
                expensePercent,
                topCategoryInfo?.Category.ToString(),
                topCategoryInfo?.Amount,
                expenseCategories,
                memberStatistic.OrderByDescending(m => m.TotalExpenses).ToList(),
                new SavingsStatisticDto
                (
                    periodContribution,
                    periodWithdrawal,
                    periodNetChange,
                    currentBalance
                )
            );
        }
    }
}

using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces;

public interface IFinanceWriter
{
    Task AddIncomeAsync(Income income, User updatedUser);
    Task DeleteIncomeAsync(Income income, User updatedUser);
    Task AddExpenseAsync(Expense expense, User updatedUser);
    Task DeleteExpenseAsync(Expense expense, User updatedUser);
    Task AddSavingsContributionAsync(SavingsContribution contribution, User updatedUser);
    Task AddSavingsWithdrawalAsync(SavingsWithdrawal withdrawal, User updatedUser);
}

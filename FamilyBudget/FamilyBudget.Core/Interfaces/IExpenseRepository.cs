using FamilyBudget.Core.Enums;
using FamilyBudget.Core.Models;

namespace FamilyBudget.Infrastructure.Repositories
{
    public interface IExpenseRepository
    {
        Task<Guid> AddAsync(Expense expense);
        Task<Guid> DeleteAsync(Guid id);
        Task<List<Expense>> GetByFamilyIdAsync(Guid familyId);
        Task<List<Expense>> GetByPeriodAsync(Guid familyId, DateTime startDate, DateTime endDate);
        Task<List<Expense>> GetByUserIdAsync(Guid userId);
        Task<Guid> UpdateAsync(Guid id, decimal amount, string description, DateTime date, ExpenseCategory category);
    }
}
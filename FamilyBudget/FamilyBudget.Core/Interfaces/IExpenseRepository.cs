using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces
{
    public interface IExpenseRepository
    {
        Task<Guid> AddAsync(Expense expense);
        Task<Guid> DeleteAsync(Guid id);
        Task<List<Expense>> GetByFamilyIdAsync(Guid familyId);
        Task<List<Expense>> GetByUserIdAsync(Guid userId);
        Task<List<Expense>> GetByPeriodAsync(Guid familyId, DateTime startDate, DateTime endDate);
        Task<Guid> UpdateAsync(Guid id, decimal amount, string description, DateTime date, Guid categoryId);
    }
}
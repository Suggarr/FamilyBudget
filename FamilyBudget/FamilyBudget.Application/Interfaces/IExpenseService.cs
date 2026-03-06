using FamilyBudget.Core.Dtos.Expense;

namespace FamilyBudget.Application.Services
{
    public interface IExpenseService
    {
        Task<Guid> AddAsync(CreateExpenseDto dto);
        Task DeleteAsync(Guid id);
        Task<List<ExpenseDto>> GetByFamilyIdAsync(Guid familyId);
        Task<List<ExpenseDto>> GetByPeriodAsync(Guid familyId, DateTime startDate, DateTime endDate);
    }
}
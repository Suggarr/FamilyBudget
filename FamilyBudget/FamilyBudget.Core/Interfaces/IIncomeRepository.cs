using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces
{
    public interface IIncomeRepository
    {
        Task<Guid> AddAsync(Income income);
        Task<List<Income>> GetByFamilyAsync(Guid familyId);
        Task<List<Income>> GetByFamilyIdAsync(Guid familyId);
        Task<List<Income>> GetByUserIdAsync(Guid userId);
        Task<Income?> GetByIdAsync(Guid id);
        Task<List<Income>> GetByPeriodAsync(Guid familyId, DateTime startDate, DateTime endDate);
        Task<Guid> UpdateAsync(Guid id, decimal amount, string source, DateTime date);
        Task<Guid> DeleteAsync(Guid id);
    }
}

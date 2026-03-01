using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces
{
    public interface IGoalRepository
    {
        Task<Guid> AddAsync(Goal goal);
        Task<IEnumerable<Goal>> GetByFamilyIdAsync(Guid familyId);
        Task<Guid> UpdateAsync(Guid id, string title, decimal targetAmount, decimal currentAmount);
        Task<Guid> DeleteAsync(Guid id);
    }
}
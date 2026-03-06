using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces
{
    public interface IGoalRepository
    {
        Task<Guid> AddAsync(Goal goal);
        Task<IEnumerable<Goal>> GetByFamilyIdAsync(Guid familyId);
        Task UpdateAsync(Goal goal);
        Task<Goal?> GetByIdAsync(Guid id);
        Task<Guid> DeleteAsync(Guid id);
    }
}
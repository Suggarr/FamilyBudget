using FamilyBudget.Core.Models;

namespace FamilyBudget.Infrastructure.Repositories
{
    public interface ICategoryRepository
    {
        Task<Guid> AddAsync(Category category);
        Task<Guid> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid familyId, string name);
        Task<List<Category>> GetByFamilyAsync(Guid familyId);
        Task<Category?> GetByIdAsync(Guid id);
        Task<Category?> GetByNameAsync(Guid familyId, string name);
        Task UpdateAsync(Category category);
    }
}
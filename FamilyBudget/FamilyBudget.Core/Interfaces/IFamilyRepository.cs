using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces
{
    public interface IFamilyRepository
    {
        Task<Guid> AddAsync(Family family);
        Task<Guid> DeleteAsync(Guid id);
        Task<List<Family>> GetAllAsync();
        Task<Family?> GetByIdAsync(Guid id);
        Task<Family?> GetByNameAsync(string name);
        Task<Guid> UpdateAsync(Guid id, string name);
    }
}
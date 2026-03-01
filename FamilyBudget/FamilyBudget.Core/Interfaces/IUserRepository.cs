using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<Guid> AddAsync(User user);
        Task<Guid> DeleteAsync(Guid id);
        Task<List<User>> GetByFamilyIdAsync(Guid familyId);
        Task<User?> GetByTelegramIdAsync(long telegramId);
        Task<Guid> UpdateAsync(Guid id, string name);
    }
}
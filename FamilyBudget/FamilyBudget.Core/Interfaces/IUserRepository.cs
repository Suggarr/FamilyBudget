using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<Guid> AddAsync(User user);
        Task<Guid> DeleteAsync(Guid id);
        Task<List<User>> GetByFamilyIdAsync(Guid familyId);
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByTelegramIdAsync(long telegramId);
        Task UpdateAsync(User user);
    }
}

using FamilyBudget.Core.Dtos.User;

namespace FamilyBudget.Application.Interfaces
{
    public interface IUserService
    {
        Task<Guid> AddUserAsync(CreateUserDto createUserDto);
        Task DeleteAsync(Guid id);
        Task<List<UserDto>> GetByFamilyIdAsync(Guid familyId);
        Task<UserDto?> GetByTelegramIdAsync(long telegramId);
        Task JoinFamilyByInvite(long telegramId, string username, Guid familyId);
        Task SetFamilyForUserAsync(long telegramId, string newName, Guid familyId);
        Task LeaveFamily(long telegramId);
    }
}
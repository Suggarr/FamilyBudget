using FamilyBudget.Core.Dtos.User;

namespace FamilyBudget.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetByFamilyIdAsync(Guid familyId);
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> GetByTelegramIdAsync(long telegramId);
    Task LeaveFamily(long telegramId);
    Task SetBalanceAsync(long telegramId, decimal balance);
    Task UpdateTelegramUsernameAsync(long telegramId, string? telegramUsername);
}

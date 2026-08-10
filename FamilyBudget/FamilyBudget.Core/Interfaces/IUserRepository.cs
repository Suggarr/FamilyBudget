using FamilyBudget.Core.Enums;
using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetByFamilyIdAsync(Guid familyId);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByExternalLoginAsync(
        ExternalLoginProvider provider,
        string providerSubject);
    Task UpdateAsync(User user);
}

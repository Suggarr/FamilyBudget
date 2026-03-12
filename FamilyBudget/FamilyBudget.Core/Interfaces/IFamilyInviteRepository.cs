using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces
{
    public interface IFamilyInviteRepository
    {
        Task AddAsync(FamilyInvite invite);
        Task DeleteAsync(Guid id);
        Task<FamilyInvite?> GetByCodeAsync(string code);
    }
}
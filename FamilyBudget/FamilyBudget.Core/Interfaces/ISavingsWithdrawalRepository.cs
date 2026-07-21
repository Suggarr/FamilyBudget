using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces;

public interface ISavingsWithdrawalRepository
{
    Task<List<SavingsWithdrawal>> GetByFamilyIdAsync(Guid familyId);
}

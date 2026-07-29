using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces;

public interface ISavingsContributionRepository
{
    Task<List<SavingsContribution>> GetByFamilyIdAsync(Guid familyId);
}

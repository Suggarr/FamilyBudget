using FamilyBudget.Application.Dtos.Savings;

namespace FamilyBudget.Application.Interfaces;

public interface ISavingsService
{
    Task ContributeAsync(Guid familyId, Guid userId, decimal amount);
    Task WithdrawAsync(Guid familyId, Guid userId, decimal amount);
    Task<SavingsSummaryDto> GetSummaryAsync(Guid familyId);
}

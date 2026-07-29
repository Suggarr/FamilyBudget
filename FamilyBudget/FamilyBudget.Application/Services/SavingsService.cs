using FamilyBudget.Application.Dtos.Savings;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;

namespace FamilyBudget.Application.Services;

public class SavingsService : ISavingsService
{
    private readonly ISavingsContributionRepository _contributionRepository;
    private readonly ISavingsWithdrawalRepository _withdrawalRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFinanceWriter _financeWriter;

    public SavingsService(
        ISavingsContributionRepository contributionRepository,
        ISavingsWithdrawalRepository withdrawalRepository,
        IUserRepository userRepository,
        IFinanceWriter financeWriter)
    {
        _contributionRepository = contributionRepository;
        _withdrawalRepository = withdrawalRepository;
        _userRepository = userRepository;
        _financeWriter = financeWriter;
    }

    public async Task ContributeAsync(Guid familyId, Guid userId, decimal amount)
    {
        var user = await GetFamilyMemberAsync(familyId, userId);
        var contributionResult = SavingsContribution.Create(Guid.NewGuid(), familyId, userId, amount, DateTime.UtcNow);
        if (contributionResult.IsFailure)
            throw new InvalidOperationException(contributionResult.Error);

        var debitResult = user.Debit(amount);
        if (debitResult.IsFailure)
            throw new InvalidOperationException(debitResult.Error);

        await _financeWriter.AddSavingsContributionAsync(contributionResult.Value, user);
    }

    public async Task WithdrawAsync(Guid familyId, Guid userId, decimal amount)
    {
        var user = await GetFamilyMemberAsync(familyId, userId);
        var withdrawalResult = SavingsWithdrawal.Create(Guid.NewGuid(), familyId, userId, amount, DateTime.UtcNow);
        if (withdrawalResult.IsFailure)
            throw new InvalidOperationException(withdrawalResult.Error);

        var creditResult = user.Credit(amount);
        if (creditResult.IsFailure)
            throw new InvalidOperationException(creditResult.Error);

        await _financeWriter.AddSavingsWithdrawalAsync(withdrawalResult.Value, user);
    }

    public async Task<SavingsSummaryDto> GetSummaryAsync(Guid familyId)
    {
        var contributions = await _contributionRepository.GetByFamilyIdAsync(familyId);
        var withdrawals = await _withdrawalRepository.GetByFamilyIdAsync(familyId);
        var users = await _userRepository.GetByFamilyIdAsync(familyId);
        var names = users.ToDictionary(u => u.Id, u => u.Name);

        var contributionItems = contributions.Select(c => new SavingsOperationDto(
            c.Id,
            c.UserId,
            names.GetValueOrDefault(c.UserId, "Бывший участник"),
            c.Amount,
            c.CreatedAt,
            SavingsOperationType.Contribution));

        var withdrawalItems = withdrawals.Select(w => new SavingsOperationDto(
            w.Id,
            w.UserId,
            names.GetValueOrDefault(w.UserId, "Бывший участник"),
            w.Amount,
            w.CreatedAt,
            SavingsOperationType.Withdrawal));

        var operations = contributionItems
            .Concat(withdrawalItems)
            .OrderByDescending(operation => operation.CreatedAt)
            .ToList();

        return new SavingsSummaryDto(
            contributions.Sum(c => c.Amount) - withdrawals.Sum(w => w.Amount),
            operations);
    }

    private async Task<User> GetFamilyMemberAsync(Guid familyId, Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        if (user.FamilyId != familyId)
            throw new InvalidOperationException("User does not belong to this family.");

        return user;
    }
}

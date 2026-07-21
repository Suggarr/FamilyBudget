using CSharpFunctionalExtensions;

namespace FamilyBudget.Core.Models;

public class SavingsWithdrawal
{
    private SavingsWithdrawal(Guid id, Guid familyId, Guid userId, decimal amount, DateTime createdAt)
    {
        Id = id;
        FamilyId = familyId;
        UserId = userId;
        Amount = amount;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public Guid FamilyId { get; }
    public Guid UserId { get; }
    public decimal Amount { get; }
    public DateTime CreatedAt { get; }

    public static Result<SavingsWithdrawal> Create(
        Guid id,
        Guid familyId,
        Guid userId,
        decimal amount,
        DateTime createdAt)
    {
        if (id == Guid.Empty || familyId == Guid.Empty || userId == Guid.Empty)
            return Result.Failure<SavingsWithdrawal>("Identifiers cannot be empty.");

        if (amount <= 0)
            return Result.Failure<SavingsWithdrawal>("Amount must be greater than zero.");

        if (createdAt > DateTime.UtcNow)
            return Result.Failure<SavingsWithdrawal>("Withdrawal date cannot be in the future.");

        return Result.Success(new SavingsWithdrawal(id, familyId, userId, amount, createdAt));
    }
}

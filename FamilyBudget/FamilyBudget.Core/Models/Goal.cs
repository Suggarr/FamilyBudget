using CSharpFunctionalExtensions;

namespace FamilyBudget.Core.Models;

public class Goal
{
    public const int MAX_TITLE_LENGTH = 200;

    private Goal(Guid id, Guid familyId, string title, decimal targetAmount, decimal currentAmount)
    {
        Id = id;
        FamilyId = familyId;
        Title = title;
        TargetAmount = targetAmount;
        CurrentAmount = currentAmount; 
    }

    public Guid Id { get; }
    public Guid FamilyId { get; }
    public string Title { get; }
    public decimal TargetAmount { get; }
    public decimal CurrentAmount { get; private set; }

    public static Result<Goal> Create(Guid id, Guid familyId, string title, decimal targetAmount, decimal currentAmount)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > MAX_TITLE_LENGTH)
        {
            return Result.Failure<Goal>($"Title can not be empty or longer than {MAX_TITLE_LENGTH} symbols");
        }
        if (targetAmount <= 0)
        {
            return Result.Failure<Goal>("Target amount must be greater than zero.");
        }
        if (currentAmount < 0)
        {
            return Result.Failure<Goal>("Current amount cannot be negative.");
        }
        if (currentAmount > targetAmount)
        {
            return Result.Failure<Goal>("Current amount cannot exceed target amount.");
        }

        var goal = new Goal(id, familyId, title.Trim(), targetAmount, currentAmount);
        return Result.Success(goal);
    }

    public Result AddMoney(decimal amount)
    {
        if (amount <= 0)
            return Result.Failure("Amount must be greater than 0");

        if (CurrentAmount + amount > TargetAmount)
            return Result.Failure("Goal cannot exceed target amount.");

        CurrentAmount += amount;
        return Result.Success();
    }

    public bool IsCompleted => CurrentAmount >= TargetAmount;

    public decimal GetProgressPercent()
    {
        return TargetAmount == 0 ? 0 : (CurrentAmount / TargetAmount) * 100;
    }
}
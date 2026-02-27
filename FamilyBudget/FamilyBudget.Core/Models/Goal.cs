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
        CurrentAmount = currentAmount; //0
    }

    public Guid Id { get; }
    public Guid FamilyId { get; }
    public string Title { get; }
    public decimal TargetAmount { get; }
    public decimal CurrentAmount { get; }

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
}
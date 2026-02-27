using CSharpFunctionalExtensions;

namespace FamilyBudget.Core.Models;

public class Expense
{
    public const int MAX_DESCRIPTION_LENGTH = 300;

    private Expense(Guid id, Guid familyId, Guid userId, Guid categoryId, decimal amount,
        string description, DateTime date)
    {
        Id = id;
        FamilyId = familyId;
        UserId = userId;
        CategoryId = categoryId;
        Amount = amount;
        Description = description;
        Date = date;
    }

    public Guid Id { get; }
    public Guid FamilyId { get; }
    public Guid UserId { get; }
    public Guid CategoryId { get; }
    public decimal Amount{ get; }
    public string Description { get; }
    public DateTime Date { get; }

    public static Result<Expense> Create(Guid id, Guid familyId, Guid userId, Guid categoryId, decimal amount,
        string description, DateTime date)
    {
        if (amount <= 0)
        { 
            return Result.Failure<Expense>("Amount must be greater than zero");
        }
        if (string.IsNullOrWhiteSpace(description) || description.Length > MAX_DESCRIPTION_LENGTH)
        {
            return Result.Failure<Expense>($"Description can not be empty or be longer than {MAX_DESCRIPTION_LENGTH} characters.");
        }

        if (date > DateTime.UtcNow)
        {
            return Result.Failure<Expense>("Date cannot be in the future");
        }

        var expense = new Expense(id, familyId, userId, categoryId, amount, description, date);
        return Result.Success(expense);
    }
}

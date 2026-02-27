using CSharpFunctionalExtensions;

namespace FamilyBudget.Core.Models;

public class Income
{
    public const int MAX_SOURCE_LENGTH = 300;

    private Income(Guid id, Guid familyId, Guid userId, decimal amount, string source, DateTime date)
    {
        Id = id;
        FamilyId = familyId;
        UserId = userId;
        Amount = amount;
        Source = source;
        Date = date;
    }

    public Guid Id { get; }
    public Guid FamilyId { get; }
    public Guid UserId { get; }
    public decimal Amount { get; }
    public string Source { get; }
    public DateTime Date { get; }

    public static Result<Income> Create(
        Guid id,
        Guid familyId,
        Guid userId,
        decimal amount,
        string source,
        DateTime date)
    {
        if (amount <= 0)
            return Result.Failure<Income>("Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(source) || source.Length > MAX_SOURCE_LENGTH)
            return Result.Failure<Income>($"Source can not be empty or longer than {MAX_SOURCE_LENGTH} symbols");

        if (date > DateTime.UtcNow)
            return Result.Failure<Income>("Date cannot be in the future.");

        var income = new Income(id, familyId, userId, amount, source, date);
        return Result.Success(income);
    }

}
namespace FamilyBudgetBot.Core.Models;

public class Income
{
    public const int MAX_SOURCE_LENGTH = 200;

    private Income() {}

    public Income(Guid id, Guid familyId, Guid userId, decimal amount, string source, DateTime date)
    {
        Id = id;
        FamilyId = familyId;
        UserId = userId;
        Amount = amount;
        Source = source;
        Date = date;
    }
    
    public Guid Id { get; private set; }
    public Guid FamilyId { get; private set; }
    public Guid UserId { get; private set; }
    
    public decimal Amount { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }

    public Family Family { get; private set; } = null!;
    public User User { get; private set; } = null!;

    public static (Income? Income, string Error) Create(
        Guid id,
        Guid familyId,
        Guid userId,
        decimal amount,
        string source,
        DateTime date)
    {
        if (id == Guid.Empty)
            return (null, "Id is required");

        if (amount <= 0)
            return (null, "Amount must be greater than zero");

        if (!string.IsNullOrEmpty(source) &&
            source.Length > MAX_SOURCE_LENGTH)
            return (null, $"Source can not exceed {MAX_SOURCE_LENGTH} symbols");

        return (new Income(id, familyId, userId, amount, source, date),
                string.Empty);
    }
}
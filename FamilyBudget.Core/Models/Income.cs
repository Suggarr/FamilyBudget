namespace FamilyBudgetBot.Core.Models;

public class Income
{
    private Income() {}

    public Income(Guid familyId, Guid userId, decimal amount, string source)
    {
        if (amount <= 0)
            throw new ArgumentException("Число должно быть положительным");

        Id = Guid.NewGuid();
        FamilyId = familyId;
        UserId = userId;
        Amount = amount;
        Source = source;
        Date = DateTime.UtcNow;
    }
    
    public Guid Id { get; private set; }
    public Guid FamilyId { get; private set; }
    public Guid UserId { get; private set; }
    
    public decimal Amount { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }

    public Family Family { get; private set; } = null!;
    public User User { get; private set; } = null!;
}
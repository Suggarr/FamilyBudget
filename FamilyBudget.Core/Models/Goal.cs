namespace FamilyBudgetBot.Core.Models;

public class Goal
{
    private Goal() {}

    public Goal(Guid familyId, string title, decimal targetAmount)
    {
        Id = Guid.NewGuid();
        FamilyId = familyId;
        Title = title;
        TargetAmount = targetAmount;
        CurrentAmount = 0;
    }
    
    public Guid Id { get; private set; }
    public Guid FamilyId { get; private set; }
    
    public string Title { get; private set; } = string.Empty;
    public decimal TargetAmount { get; private set; }
    public decimal CurrentAmount { get; private set; }
    
    // public DateTime Date { get; private set; }

    public Family Family { get; private set; } = null!;

    public void AddFunds(decimal amount)
    {
        if (amount <= 0)
            CurrentAmount += amount;
    }
}
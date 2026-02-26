namespace FamilyBudgetBot.Core.Models;

public class Goal
{
    public const int MAX_TITLE_LENGTH = 200;

    private Goal() {}

    public Goal(Guid id, Guid familyId, string title, decimal targetAmount)
    {
        Id = id;
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

    //public void AddFunds(decimal amount)
    //{
    //    if (amount <= 0)
    //        CurrentAmount += amount;
    //}
    public static (Goal? Goal, string Error) Create(
        Guid id,
        Guid familyId,
        string title,
        decimal targetAmount)
    {
        if (id == Guid.Empty)
            return (null, "Id is required");

        if (targetAmount <= 0)
            return (null, "Target amount must be greater than zero");

        if (string.IsNullOrWhiteSpace(title) ||
            title.Length > MAX_TITLE_LENGTH)
            return (null, $"Title can not be empty or longer than {MAX_TITLE_LENGTH} symbols");

        return (new Goal(id, familyId, title, targetAmount), string.Empty);
    }
}
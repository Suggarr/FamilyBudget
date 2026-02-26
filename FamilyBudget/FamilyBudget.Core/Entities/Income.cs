namespace FamilyBudgetBot.Core.Entities;

public class Income
{ 
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal Amount { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
namespace FamilyBudgetBot.Core.Entities;

public class Receipt
{ 
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string FilePath { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }
    public decimal? TotalAmount { get; set; }
}
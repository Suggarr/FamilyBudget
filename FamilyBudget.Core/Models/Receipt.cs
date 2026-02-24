namespace FamilyBudgetBot.Core.Models;

public class Receipt
{
    private Receipt() {}

    public Receipt(Guid id, Guid familyId, Guid userId, string filePath)
    {
        Id = id;
        FamilyId = familyId;
        UserId = userId;
        FilePath = filePath;
        IsProcessed = false;
    }
    
    public Guid Id { get; private set; }
    public Guid FamilyId { get; private set; }
    public Guid UserId { get; private set; }
    
    public string FilePath { get; private set; } = string.Empty;
    public bool IsProcessed { get; private set; }
    public decimal? TotalAmount { get; private set; }

    public Family Family { get; private set; } = null!;
    public User User { get; private set; } = null!;

    public void MarkAsProcessed(decimal totalAmount)
    {
        IsProcessed = true;
        TotalAmount = totalAmount;
    }
}
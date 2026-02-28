namespace FamilyBudget.Infrastructure.Entities;

public class GoalEntity
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }
    public FamilyEntity Family { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
}
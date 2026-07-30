namespace FamilyBudget.Persistence.Entities;

public class SavingsContributionEntity
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }

    public FamilyEntity Family { get; set; } = null!;
    public UserEntity User { get; set; } = null!;
}

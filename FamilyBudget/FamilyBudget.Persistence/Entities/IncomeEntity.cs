namespace FamilyBudget.Persistence.Entities;

public class IncomeEntity
{ 
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }
    public FamilyEntity Family { get; set; } = null!;

    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public decimal Amount { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
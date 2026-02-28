namespace FamilyBudget.Infrastructure.Entities;

public class ExpenseEntity
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }
    public FamilyEntity Family = null!;

    public Guid UserId { get; set; }
    public UserEntity User = null!;

    public Guid CategoryId { get; set; }
    public CategoryEntity Category = null!;

    public decimal Amount{ get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

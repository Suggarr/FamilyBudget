namespace FamilyBudget.Infrastructure.Entities;

public class CategoryEntity
{
    public const int MAX_NAME_LENGTH = 100;

    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }
    public FamilyEntity Family { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public ICollection<ExpenseEntity> Expenses { get; set; } = new List<ExpenseEntity>();
}
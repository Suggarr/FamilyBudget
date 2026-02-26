namespace FamilyBudgetBot.Core.Entities;

public class Category
{
    public const int MAX_NAME_LENGTH = 100;

    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
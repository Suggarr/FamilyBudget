namespace FamilyBudget.Infrastructure.Entities;

public class FamilyEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
    public ICollection<CategoryEntity> Categories { get; set; } = new List<CategoryEntity>();
    public ICollection<ExpenseEntity> Expenses { get; set; } = new List<ExpenseEntity>();
    public ICollection<IncomeEntity> Incomes { get; set; } = new List<IncomeEntity>();
    public ICollection<GoalEntity> Goals { get; set; } = new List<GoalEntity>();
}
namespace FamilyBudget.Infrastructure.Entities;

public class UserEntity
{
    public const int MAX_NAME_LENGTH = 100;

    public Guid Id { get; set; }

    public Guid? FamilyId { get; set;}
    public FamilyEntity? Family { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public long TelegramId { get; set; }

    public ICollection<ExpenseEntity> Expenses { get; set; } = new List<ExpenseEntity>();
    public ICollection<IncomeEntity> Incomes { get; set; } = new List<IncomeEntity>();
}
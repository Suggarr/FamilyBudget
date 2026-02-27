namespace FamilyBudget.Infrastructure.Entities;

public class User
{
    public const int MAX_NAME_LENGTH = 100;

    public Guid Id { get; set; }

    public Guid FamilyId { get; set;}
    public Family Family { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public long TelegramId { get; set; }

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Income> Incomes { get; set; } = new List<Income>();
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
}
namespace FamilyBudgetBot.Core.Models;

public class Expense
{
    public const int MAX_DESCRIPTION_LENGTH = 300;

    private Expense() {}
    
    public Expense(Guid id, Guid familyId, Guid userId, Guid categoryId, decimal amount,
        string description, DateTime date)
    {
        Id = id;
        FamilyId = familyId;
        UserId = userId;
        CategoryId = categoryId;
        Amount = amount;
        Description = description;
        Date = date;
    }
    
    public Guid Id { get; private set; }
    public Guid FamilyId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    
    public decimal Amount{ get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    
    public Family Family = null!;
    public User User = null!;
    public Category Category = null!;

    public static (Expense Expense, string Error) Create(Guid id, Guid familyId, Guid userId, Guid categoryId,
        decimal amount, string description)
    {
        var error = string.Empty;
        if (familyId == Guid.Empty)
        {
            error = "FamilyId is required";
        }

        if (userId == Guid.Empty)
        {
            error = "UserId is required";
        }

        if (categoryId == Guid.Empty)
        {
            error = "CategoryId is required";
        }

        if (amount <= 0)
        {
            error = "Amount must be greater than zero";
        }
        if (string.IsNullOrWhiteSpace(description) || description.Length > MAX_DESCRIPTION_LENGTH)
        {
            error = $"Name can not be empty or longer than {MAX_DESCRIPTION_LENGTH} symbols";
        }

        var expense = new Expense(id, familyId, userId, categoryId, amount,
            description, DateTime.UtcNow);

        return (expense, error);
    }
}

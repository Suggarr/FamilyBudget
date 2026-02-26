namespace FamilyBudgetBot.Core.Models;

public class User
{
    public const int MAX_NAME_LENGTH = 100;

    private User() {}
    
    private User(Guid id,Guid familyId, string name, long telegramId)
    {
        Id = id;
        FamilyId = familyId;
        Name = name;
        TelegramId = telegramId;
    }

    public Guid Id { get; set; }

    public Guid FamilyId { get; set;}
    public Family Family { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public long TelegramId { get; set; }

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();

    public static (User User, string Error) Create(Guid id, Guid familyId, string name, long telegramId)
    {
        var error = string.Empty;
        if (familyId == Guid.Empty)
        {
            error = "FamilyId is required";
        }
        if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
        {
            error = $"Name can not be empty or longer than {MAX_NAME_LENGTH} symbols";
        }

        var user = new User(id, familyId, name, telegramId);
        return (user, error);
    }
}
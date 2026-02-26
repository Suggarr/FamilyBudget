namespace FamilyBudgetBot.Core.Models;

public class Family
{
    public const int MAX_NAME_LENGTH = 100;

    private Family() {}
    
    public Family(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();

    public static (Family Family, string Error) Create(Guid id, string name)
    {
        var error = string.Empty;

        if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
        {
            error = $"Name can not be empty or longer than {MAX_NAME_LENGTH} symbols";
        }

        var family = new Family(id, name);
        return (family, error);
    }
}
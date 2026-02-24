namespace FamilyBudgetBot.Core.Models;

public class Family
{
    private Family() {}
    
    public Family(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();

    // public static (Family Family, string Error) Create(Guid id, string name)
    // {
    //     var error = string.Empty;
    //     if (string.IsNullOrWhiteSpace(name))
    //     {
    //         error = "Family name is required";
    //     }
    //     if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
    //     {
    //         error = $"Name can not be empty or longer than {MAX_NAME_LENGTH} symbols";
    //     }
    //
    //     var family = new Family(id, name);
    //     return (family, error);
    // }
}
namespace FamilyBudgetBot.Core.Models;

public class Category
{
    private Category() {}

    public Category(Guid familyId, string name)
    {
        Id = Guid.NewGuid();
        FamilyId = familyId;
        Name = name;
    }
    
    public Guid Id { get; private set; }
    public Guid FamilyId { get; private set; }
    public Family Family { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public static (Category? Category, string Error) Create(
        Guid id,
        Guid familyId,
        string name)
    {
        if (id == Guid.Empty)
            return (null, "Id is required");

        if (familyId == Guid.Empty)
            return (null, "FamilyId is required");

        if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
            return (null, $"Name can not be empty or longer than {MAX_NAME_LENGTH} symbols");

        return (new Category(id, familyId, name), string.Empty);
    }
}
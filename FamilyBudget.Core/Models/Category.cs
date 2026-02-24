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
}
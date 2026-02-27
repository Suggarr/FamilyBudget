using CSharpFunctionalExtensions;

namespace FamilyBudget.Core.Models;

public class Family
{
    public const int MAX_NAME_LENGTH = 100;

    private Family(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; }
    public string Name { get; }

    public static Result<Family> Create(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            return Result.Failure<Family>($"Family name can not be empty or longer than {MAX_NAME_LENGTH} symbols");
        }
        var family = new Family(id, name.Trim());
        return Result.Success(family);
    }
}
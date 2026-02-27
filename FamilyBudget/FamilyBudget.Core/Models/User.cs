using CSharpFunctionalExtensions;

namespace FamilyBudget.Core.Models;

public class User
{
    public const int MAX_NAME_LENGTH = 100;

    private User(Guid id, Guid familyId, string name, long telegramId)
    {
        Id = id;
        FamilyId = familyId;
        Name = name;
        TelegramId = telegramId;
    }

    public Guid Id { get; }
    public Guid FamilyId { get; }
    public string Name { get; }
    public long TelegramId { get; }

    public static Result<User> Create(Guid id, Guid familyId, string name, long telegramId)
    {
        if (telegramId <= 0)
        {
            return Result.Failure<User>("Invalid TelegramId.");
        }

        if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
        {
            return Result.Failure<User>($"Name can not be empty or longer than {MAX_NAME_LENGTH} symbols");
        }

        var user = new User(id, familyId, name.Trim(), telegramId);
        return Result.Success(user);
    }
}
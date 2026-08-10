using CSharpFunctionalExtensions;

namespace FamilyBudget.Core.Models;

public class User
{
    public const int MAX_NAME_LENGTH = 100;

    private User(Guid id, Guid? familyId, string name, decimal balance)
    {
        Id = id;
        FamilyId = familyId;
        Name = name;
        Balance = balance;
    }

    public Guid Id { get; private set; }
    public Guid? FamilyId { get; private set; }
    public string Name { get; private set; }
    public decimal Balance { get; private set; }

    public static Result<User> Create(
        Guid id,
        Guid? familyId,
        string name,
        decimal balance = 0)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<User>("Id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
        {
            return Result.Failure<User>($"Name can not be empty or longer than {MAX_NAME_LENGTH} symbols");
        }

        if (balance < 0)
        {
            return Result.Failure<User>("Balance cannot be negative.");
        }

        var user = new User(id, familyId, name.Trim(), balance);
        return Result.Success(user);
    }

    public Result JoinFamily(Guid familyId, string name)
    {
        if (familyId == Guid.Empty)
            return Result.Failure("FamilyId cannot be empty.");

        if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
            return Result.Failure($"Name can not be empty or longer than {MAX_NAME_LENGTH} symbols");

        FamilyId = familyId;
        Name = name.Trim();
        return Result.Success();
    }

    public void LeaveFamily()
    {
        FamilyId = null;
    }

    public Result SetBalance(decimal balance)
    {
        if (balance < 0)
            return Result.Failure("Balance cannot be negative.");

        Balance = balance;
        return Result.Success();
    }

    public Result Credit(decimal amount)
    {
        if (amount <= 0)
            return Result.Failure("Amount must be greater than zero.");

        Balance += amount;
        return Result.Success();
    }

    public Result Debit(decimal amount)
    {
        if (amount <= 0)
            return Result.Failure("Amount must be greater than zero.");

        if (amount > Balance)
            return Result.Failure("Insufficient funds.");

        Balance -= amount;
        return Result.Success();
    }
}

using CSharpFunctionalExtensions;

namespace FamilyBudget.Core.Models.Auth;

public sealed class Account
{
    public const int MaxDisplayNameLength = 200;

    private Account(
        Guid id,
        string displayName,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? disabledAtUtc)
    {
        Id = id;
        DisplayName = displayName;
        CreatedAtUtc = createdAtUtc;
        DisabledAtUtc = disabledAtUtc;
    }

    public Guid Id { get; }
    public string DisplayName { get; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset? DisabledAtUtc { get; private set; }
    public bool IsActive => !DisabledAtUtc.HasValue;

    public static Result<Account> Create(
        Guid id,
        string displayName,
        DateTimeOffset createdAtUtc)
    {
        var validation = Validate(id, displayName, createdAtUtc, null);
        if (validation.IsFailure)
        {
            return Result.Failure<Account>(validation.Error);
        }

        return Result.Success(new Account(
            id,
            displayName.Trim(),
            createdAtUtc,
            null));
    }

    public static Result<Account> Restore(
        Guid id,
        string displayName,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? disabledAtUtc)
    {
        var validation = Validate(id, displayName, createdAtUtc, disabledAtUtc);
        if (validation.IsFailure)
        {
            return Result.Failure<Account>(validation.Error);
        }

        return Result.Success(new Account(
            id,
            displayName.Trim(),
            createdAtUtc,
            disabledAtUtc));
    }

    public Result Disable(DateTimeOffset utcNow)
    {
        if (DisabledAtUtc.HasValue)
        {
            return Result.Success();
        }

        if (utcNow.Offset != TimeSpan.Zero)
        {
            return Result.Failure("UtcNow must be in UTC.");
        }

        if (utcNow < CreatedAtUtc)
        {
            return Result.Failure("DisabledAtUtc cannot be before CreatedAtUtc.");
        }

        DisabledAtUtc = utcNow;
        return Result.Success();
    }

    private static Result Validate(
        Guid id,
        string displayName,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? disabledAtUtc)
    {
        var normalizedDisplayName = displayName?.Trim();

        if (id == Guid.Empty)
        {
            return Result.Failure("Id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(normalizedDisplayName))
        {
            return Result.Failure("DisplayName cannot be empty.");
        }

        if (normalizedDisplayName.Length > MaxDisplayNameLength)
        {
            return Result.Failure(
                $"DisplayName cannot be longer than {MaxDisplayNameLength} symbols.");
        }

        if (createdAtUtc.Offset != TimeSpan.Zero ||
            (disabledAtUtc.HasValue && disabledAtUtc.Value.Offset != TimeSpan.Zero))
        {
            return Result.Failure("All DateTimeOffset values must be in UTC.");
        }

        if (disabledAtUtc.HasValue && disabledAtUtc.Value < createdAtUtc)
        {
            return Result.Failure("DisabledAtUtc cannot be before CreatedAtUtc.");
        }

        return Result.Success();
    }
}

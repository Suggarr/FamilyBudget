using System.Net.Mail;
using CSharpFunctionalExtensions;

namespace FamilyBudget.Core.Models.Auth;

public sealed class LocalCredential
{
    public const int MaxEmailLength = 320;
    public const int MaxPasswordHashLength = 1024;

    private LocalCredential(
        Guid accountId,
        string email,
        string normalizedEmail,
        string passwordHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? passwordChangedAtUtc)
    {
        AccountId = accountId;
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        CreatedAtUtc = createdAtUtc;
        PasswordChangedAtUtc = passwordChangedAtUtc;
    }

    public Guid AccountId { get; }
    public string Email { get; }
    public string NormalizedEmail { get; }
    public string PasswordHash { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset? PasswordChangedAtUtc { get; private set; }

    public static Result<LocalCredential> Create(
        Guid accountId,
        string email,
        string passwordHash,
        DateTimeOffset createdAtUtc)
    {
        var validation = Validate(
            accountId,
            email,
            passwordHash,
            createdAtUtc,
            null);

        if (validation.IsFailure)
        {
            return Result.Failure<LocalCredential>(validation.Error);
        }

        var normalizedEmail = NormalizeEmail(email);
        return Result.Success(new LocalCredential(
            accountId,
            email.Trim(),
            normalizedEmail,
            passwordHash.Trim(),
            createdAtUtc,
            null));
    }

    public static Result<LocalCredential> Restore(
        Guid accountId,
        string email,
        string passwordHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? passwordChangedAtUtc)
    {
        var validation = Validate(
            accountId,
            email,
            passwordHash,
            createdAtUtc,
            passwordChangedAtUtc);

        if (validation.IsFailure)
        {
            return Result.Failure<LocalCredential>(validation.Error);
        }

        var normalizedEmail = NormalizeEmail(email);
        return Result.Success(new LocalCredential(
            accountId,
            email.Trim(),
            normalizedEmail,
            passwordHash.Trim(),
            createdAtUtc,
            passwordChangedAtUtc));
    }

    public Result ChangePassword(string passwordHash, DateTimeOffset utcNow)
    {
        var normalizedPasswordHash = passwordHash?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedPasswordHash))
        {
            return Result.Failure("PasswordHash cannot be empty.");
        }

        if (normalizedPasswordHash.Length > MaxPasswordHashLength)
        {
            return Result.Failure(
                $"PasswordHash cannot be longer than {MaxPasswordHashLength} symbols.");
        }

        if (utcNow.Offset != TimeSpan.Zero)
        {
            return Result.Failure("UtcNow must be in UTC.");
        }

        if (utcNow < CreatedAtUtc ||
            (PasswordChangedAtUtc.HasValue && utcNow < PasswordChangedAtUtc.Value))
        {
            return Result.Failure("PasswordChangedAtUtc cannot move backwards.");
        }

        PasswordHash = normalizedPasswordHash;
        PasswordChangedAtUtc = utcNow;
        return Result.Success();
    }

    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static Result Validate(
        Guid accountId,
        string email,
        string passwordHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? passwordChangedAtUtc)
    {
        var normalizedEmail = email?.Trim();
        var normalizedPasswordHash = passwordHash?.Trim();

        if (accountId == Guid.Empty)
        {
            return Result.Failure("AccountId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail) ||
            normalizedEmail.Length > MaxEmailLength ||
            !MailAddress.TryCreate(normalizedEmail, out _))
        {
            return Result.Failure("Email is invalid.");
        }

        if (string.IsNullOrWhiteSpace(normalizedPasswordHash))
        {
            return Result.Failure("PasswordHash cannot be empty.");
        }

        if (normalizedPasswordHash.Length > MaxPasswordHashLength)
        {
            return Result.Failure(
                $"PasswordHash cannot be longer than {MaxPasswordHashLength} symbols.");
        }

        if (createdAtUtc.Offset != TimeSpan.Zero ||
            (passwordChangedAtUtc.HasValue && passwordChangedAtUtc.Value.Offset != TimeSpan.Zero))
        {
            return Result.Failure("All DateTimeOffset values must be in UTC.");
        }

        if (passwordChangedAtUtc.HasValue && passwordChangedAtUtc.Value < createdAtUtc)
        {
            return Result.Failure(
                "PasswordChangedAtUtc cannot be before CreatedAtUtc.");
        }

        return Result.Success();
    }
}

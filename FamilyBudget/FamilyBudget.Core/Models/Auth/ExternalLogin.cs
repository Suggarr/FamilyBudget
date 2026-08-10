using CSharpFunctionalExtensions;
using FamilyBudget.Core.Enums;

namespace FamilyBudget.Core.Models.Auth;

public sealed class ExternalLogin
{
    public const int MaxProviderSubjectLength = 200;
    public const int MaxProviderUsernameLength = 200;

    private ExternalLogin(
        Guid id,
        Guid accountId,
        ExternalLoginProvider provider,
        string providerSubject,
        string? providerUsername,
        DateTimeOffset createdAtUtc,
        DateTimeOffset lastSeenAtUtc)
    {
        Id = id;
        AccountId = accountId;
        Provider = provider;
        ProviderSubject = providerSubject;
        ProviderUsername = providerUsername;
        CreatedAtUtc = createdAtUtc;
        LastSeenAtUtc = lastSeenAtUtc;
    }

    public Guid Id { get; }
    public Guid AccountId { get; }
    public ExternalLoginProvider Provider { get; }
    public string ProviderSubject { get; }
    public string? ProviderUsername { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset LastSeenAtUtc { get; private set; }

    public static Result<ExternalLogin> Create(
        Guid id,
        Guid accountId,
        ExternalLoginProvider provider,
        string providerSubject,
        string? providerUsername,
        DateTimeOffset createdAtUtc)
    {
        var validation = Validate(
            id,
            accountId,
            provider,
            providerSubject,
            providerUsername,
            createdAtUtc,
            createdAtUtc);

        if (validation.IsFailure)
        {
            return Result.Failure<ExternalLogin>(validation.Error);
        }

        return Result.Success(new ExternalLogin(
            id,
            accountId,
            provider,
            providerSubject.Trim(),
            NormalizeUsername(providerUsername),
            createdAtUtc,
            createdAtUtc));
    }

    public static Result<ExternalLogin> Restore(
        Guid id,
        Guid accountId,
        ExternalLoginProvider provider,
        string providerSubject,
        string? providerUsername,
        DateTimeOffset createdAtUtc,
        DateTimeOffset lastSeenAtUtc)
    {
        var validation = Validate(
            id,
            accountId,
            provider,
            providerSubject,
            providerUsername,
            createdAtUtc,
            lastSeenAtUtc);

        if (validation.IsFailure)
        {
            return Result.Failure<ExternalLogin>(validation.Error);
        }

        return Result.Success(new ExternalLogin(
            id,
            accountId,
            provider,
            providerSubject.Trim(),
            NormalizeUsername(providerUsername),
            createdAtUtc,
            lastSeenAtUtc));
    }

    public Result Touch(DateTimeOffset utcNow, string? providerUsername)
    {
        if (utcNow.Offset != TimeSpan.Zero)
        {
            return Result.Failure("UtcNow must be in UTC.");
        }

        if (utcNow < LastSeenAtUtc)
        {
            return Result.Failure("LastSeenAtUtc cannot move backwards.");
        }

        var normalizedUsername = NormalizeUsername(providerUsername);
        if (normalizedUsername?.Length > MaxProviderUsernameLength)
        {
            return Result.Failure(
                $"ProviderUsername cannot be longer than {MaxProviderUsernameLength} symbols.");
        }

        ProviderUsername = normalizedUsername;
        LastSeenAtUtc = utcNow;
        return Result.Success();
    }

    private static Result Validate(
        Guid id,
        Guid accountId,
        ExternalLoginProvider provider,
        string providerSubject,
        string? providerUsername,
        DateTimeOffset createdAtUtc,
        DateTimeOffset lastSeenAtUtc)
    {
        var normalizedSubject = providerSubject?.Trim();
        var normalizedUsername = NormalizeUsername(providerUsername);

        if (id == Guid.Empty)
        {
            return Result.Failure("Id cannot be empty.");
        }

        if (accountId == Guid.Empty)
        {
            return Result.Failure("AccountId cannot be empty.");
        }

        if (!Enum.IsDefined(provider))
        {
            return Result.Failure("External login provider is invalid.");
        }

        if (string.IsNullOrWhiteSpace(normalizedSubject))
        {
            return Result.Failure("ProviderSubject cannot be empty.");
        }

        if (normalizedSubject.Length > MaxProviderSubjectLength)
        {
            return Result.Failure(
                $"ProviderSubject cannot be longer than {MaxProviderSubjectLength} symbols.");
        }

        if (normalizedUsername?.Length > MaxProviderUsernameLength)
        {
            return Result.Failure(
                $"ProviderUsername cannot be longer than {MaxProviderUsernameLength} symbols.");
        }

        if (createdAtUtc.Offset != TimeSpan.Zero || lastSeenAtUtc.Offset != TimeSpan.Zero)
        {
            return Result.Failure("All DateTimeOffset values must be in UTC.");
        }

        if (lastSeenAtUtc < createdAtUtc)
        {
            return Result.Failure("LastSeenAtUtc cannot be before CreatedAtUtc.");
        }

        return Result.Success();
    }

    private static string? NormalizeUsername(string? providerUsername)
    {
        if (string.IsNullOrWhiteSpace(providerUsername))
        {
            return null;
        }

        return providerUsername.Trim().TrimStart('@');
    }
}

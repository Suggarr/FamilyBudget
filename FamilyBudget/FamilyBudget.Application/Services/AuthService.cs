using FamilyBudget.Application.Dtos.Auth;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Exceptions;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Core.Models.Auth;

namespace FamilyBudget.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IAccountRepository _accounts;
    private readonly IAccountRegistrationWriter _writer;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IAccountRepository accounts,
        IAccountRegistrationWriter writer,
        IPasswordHasher passwordHasher)
    {
        _accounts = accounts;
        _writer = writer;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> RegisterAsync(
        RegisterLocalRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = LocalCredential.NormalizeEmail(request.Email);
        if (await _accounts.GetCredentialByNormalizedEmailAsync(
                normalizedEmail, cancellationToken) is not null)
        {
            throw new EmailAlreadyRegisteredException();
        }

        var accountId = Guid.NewGuid();
        var utcNow = DateTimeOffset.UtcNow;

        var accountResult = Account.Create(accountId, request.DisplayName, utcNow);
        if (accountResult.IsFailure)
        {
            throw new ArgumentException(accountResult.Error, nameof(request));
        }

        var userResult = User.Create(accountId, null, request.DisplayName);
        if (userResult.IsFailure)
        {
            throw new ArgumentException(userResult.Error, nameof(request));
        }

        var passwordHash = _passwordHasher.Hash(accountResult.Value, request.Password);
        var credentialResult = LocalCredential.Create(
            accountId, request.Email, passwordHash, utcNow);
        if (credentialResult.IsFailure)
        {
            throw new ArgumentException(credentialResult.Error, nameof(request));
        }

        await _writer.CreateAsync(
            accountResult.Value,
            credentialResult.Value,
            userResult.Value,
            cancellationToken);

        return accountId;
    }

    private async Task<Account?> ValidateCredential(LoginRequest loginRequest, CancellationToken cancellationToken)
    {
        var normalizedEmail = LocalCredential.NormalizeEmail(loginRequest.Email);

        var credential = await _accounts.GetCredentialByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (credential is null)
        {
            return null;
        }

        var account = await _accounts.GetByIdAsync(credential.AccountId, cancellationToken);

        if (account is null || !account.IsActive)
        {
            return null;
        }

        return _passwordHasher.Verify(account, credential.PasswordHash, loginRequest.Password) ? account : null;
    }
}

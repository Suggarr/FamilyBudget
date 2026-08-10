using System.Globalization;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Enums;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Core.Models.Auth;

namespace FamilyBudget.Application.Services;

public sealed class RegistrationService : IRegistrationService
{
    private readonly IUserRepository _userRepository;
    private readonly IExternalLoginRepository _externalLoginRepository;
    private readonly IFamilyMembershipWriter _membershipWriter;

    public RegistrationService(
        IUserRepository userRepository,
        IExternalLoginRepository externalLoginRepository,
        IFamilyMembershipWriter membershipWriter)
    {
        _userRepository = userRepository;
        _externalLoginRepository = externalLoginRepository;
        _membershipWriter = membershipWriter;
    }

    public async Task<bool> IsRegisteredAsync(long telegramId)
    {
        var user = await _userRepository.GetByExternalLoginAsync(
            ExternalLoginProvider.Telegram,
            ToTelegramSubject(telegramId));

        return user is not null;
    }

    public async Task RegisterNewFamilyAsync(
        long telegramId,
        string familyName,
        string userName,
        string? telegramUsername)
    {
        var providerSubject = ToTelegramSubject(telegramId);
        var existingUser = await _userRepository.GetByExternalLoginAsync(
            ExternalLoginProvider.Telegram,
            providerSubject);
        var existingLogin = await _externalLoginRepository.GetAsync(
            ExternalLoginProvider.Telegram,
            providerSubject);

        if (existingUser is null && existingLogin is not null)
        {
            throw new InvalidOperationException(
                "Account is disabled or its user profile is unavailable.");
        }

        if (existingUser?.FamilyId is not null)
        {
            throw new InvalidOperationException("User already belongs to a family.");
        }

        var familyResult = Family.Create(Guid.NewGuid(), familyName);
        if (familyResult.IsFailure)
        {
            throw new InvalidOperationException(familyResult.Error);
        }

        var utcNow = DateTimeOffset.UtcNow;

        if (existingUser is not null)
        {
            var joinResult = existingUser.JoinFamily(familyResult.Value.Id, userName);
            if (joinResult.IsFailure)
            {
                throw new InvalidOperationException(joinResult.Error);
            }

            var externalLogin = existingLogin
                ?? throw new InvalidOperationException("Telegram login was not found.");
            var touchResult = externalLogin.Touch(utcNow, telegramUsername);
            if (touchResult.IsFailure)
            {
                throw new InvalidOperationException(touchResult.Error);
            }

            await _membershipWriter.CreateFamilyAsync(
                familyResult.Value,
                existingUser,
                null,
                externalLogin);
            return;
        }

        var accountId = Guid.NewGuid();
        var accountResult = Account.Create(accountId, userName, utcNow);
        var userResult = User.Create(accountId, familyResult.Value.Id, userName);
        var loginResult = ExternalLogin.Create(
            Guid.NewGuid(),
            accountId,
            ExternalLoginProvider.Telegram,
            providerSubject,
            telegramUsername,
            utcNow);

        if (accountResult.IsFailure)
        {
            throw new InvalidOperationException(accountResult.Error);
        }

        if (userResult.IsFailure)
        {
            throw new InvalidOperationException(userResult.Error);
        }

        if (loginResult.IsFailure)
        {
            throw new InvalidOperationException(loginResult.Error);
        }

        await _membershipWriter.CreateFamilyAsync(
            familyResult.Value,
            userResult.Value,
            accountResult.Value,
            loginResult.Value);
    }

    private static string ToTelegramSubject(long telegramId)
    {
        if (telegramId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(telegramId),
                "TelegramId must be greater than zero.");
        }

        return telegramId.ToString(CultureInfo.InvariantCulture);
    }
}

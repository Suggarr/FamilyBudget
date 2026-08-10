using System.Globalization;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Dtos.Family;
using FamilyBudget.Core.Enums;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Core.Models.Auth;

namespace FamilyBudget.Application.Services;

public sealed class FamilyInviteService : IFamilyInviteService
{
    private readonly IFamilyInviteRepository _familyInviteRepository;
    private readonly IUserRepository _userRepository;
    private readonly IExternalLoginRepository _externalLoginRepository;
    private readonly IFamilyMembershipWriter _membershipWriter;

    public FamilyInviteService(
        IFamilyInviteRepository familyInviteRepository,
        IUserRepository userRepository,
        IExternalLoginRepository externalLoginRepository,
        IFamilyMembershipWriter membershipWriter)
    {
        _familyInviteRepository = familyInviteRepository;
        _userRepository = userRepository;
        _externalLoginRepository = externalLoginRepository;
        _membershipWriter = membershipWriter;
    }

    public async Task<string> CreateInvite(Guid familyId)
    {
        var invite = FamilyInvite.Create(
            Guid.NewGuid(),
            familyId,
            string.Empty,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(24)).Value;

        invite.GenerateCode();
        invite.SetLifeTime(TimeSpan.FromHours(24));
        await _familyInviteRepository.AddAsync(invite);
        return invite.Code;
    }

    public async Task<bool> IsInviteValid(string code)
    {
        var invite = await _familyInviteRepository.GetByCodeAsync(code);
        return invite is not null && !invite.IsExpired;
    }

    public async Task<JoinFamilyResult> JoinFamilyAsync(
        string code,
        long telegramId,
        string userName,
        string? telegramUsername)
    {
        var invite = await _familyInviteRepository.GetByCodeAsync(code);
        if (invite is null || invite.IsExpired)
        {
            return JoinFamilyResult.Failure("Код приглашения недействителен или истёк.");
        }

        var providerSubject = ToTelegramSubject(telegramId);
        var user = await _userRepository.GetByExternalLoginAsync(
            ExternalLoginProvider.Telegram,
            providerSubject);
        var existingLogin = await _externalLoginRepository.GetAsync(
            ExternalLoginProvider.Telegram,
            providerSubject);

        if (user is null && existingLogin is not null)
        {
            return JoinFamilyResult.Failure("Учётная запись отключена.");
        }

        if (user is not null && user.FamilyId.HasValue && user.FamilyId != invite.FamilyId)
        {
            return JoinFamilyResult.Failure("Сначала покиньте текущую семью.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        Account? newAccount = null;
        ExternalLogin externalLogin;

        if (user is null)
        {
            var accountId = Guid.NewGuid();
            var accountResult = Account.Create(accountId, userName, utcNow);
            var userResult = User.Create(accountId, invite.FamilyId, userName);
            var loginResult = ExternalLogin.Create(
                Guid.NewGuid(),
                accountId,
                ExternalLoginProvider.Telegram,
                providerSubject,
                telegramUsername,
                utcNow);

            if (accountResult.IsFailure)
            {
                return JoinFamilyResult.Failure(accountResult.Error);
            }

            if (userResult.IsFailure)
            {
                return JoinFamilyResult.Failure(userResult.Error);
            }

            if (loginResult.IsFailure)
            {
                return JoinFamilyResult.Failure(loginResult.Error);
            }

            newAccount = accountResult.Value;
            user = userResult.Value;
            externalLogin = loginResult.Value;
        }
        else
        {
            var joinResult = user.JoinFamily(invite.FamilyId, userName);
            if (joinResult.IsFailure)
            {
                return JoinFamilyResult.Failure(joinResult.Error);
            }

            externalLogin = existingLogin
                ?? throw new InvalidOperationException("Telegram login was not found.");

            var touchResult = externalLogin.Touch(utcNow, telegramUsername);
            if (touchResult.IsFailure)
            {
                return JoinFamilyResult.Failure(touchResult.Error);
            }
        }

        try
        {
            await _membershipWriter.JoinByInviteAsync(
                invite,
                user,
                newAccount,
                externalLogin);
        }
        catch (InvalidOperationException exception)
            when (exception.Message == "Invite has already been used or expired.")
        {
            return JoinFamilyResult.Failure("Код приглашения уже использован или истёк.");
        }
        catch (InvalidOperationException exception)
            when (exception.Message ==
                  "User no longer exists or belongs to another family.")
        {
            return JoinFamilyResult.Failure(
                "Пользователь уже вступил в другую семью.");
        }

        return JoinFamilyResult.Success();
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

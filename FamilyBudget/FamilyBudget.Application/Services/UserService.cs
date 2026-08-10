using System.Globalization;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Dtos.User;
using FamilyBudget.Core.Enums;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Core.Models.Auth;

namespace FamilyBudget.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IExternalLoginRepository _externalLoginRepository;

    public UserService(
        IUserRepository userRepository,
        IExternalLoginRepository externalLoginRepository)
    {
        _userRepository = userRepository;
        _externalLoginRepository = externalLoginRepository;
    }

    public async Task<UserDto?> GetByTelegramIdAsync(long telegramId)
    {
        var providerSubject = ToTelegramSubject(telegramId);
        var user = await _userRepository.GetByExternalLoginAsync(
            ExternalLoginProvider.Telegram,
            providerSubject);

        if (user is null)
        {
            return null;
        }

        var login = await _externalLoginRepository.GetAsync(
            ExternalLoginProvider.Telegram,
            providerSubject);

        return ToDto(user, login);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return null;
        }

        var login = await _externalLoginRepository.GetByAccountIdAsync(
            id,
            ExternalLoginProvider.Telegram);

        return ToDto(user, login);
    }

    public async Task<List<UserDto>> GetByFamilyIdAsync(Guid familyId)
    {
        var users = await _userRepository.GetByFamilyIdAsync(familyId);
        var logins = await _externalLoginRepository.GetByAccountIdsAsync(
            users.Select(user => user.Id).ToArray(),
            ExternalLoginProvider.Telegram);
        var loginsByAccountId = logins.ToDictionary(login => login.AccountId);

        return users
            .Select(user => ToDto(
                user,
                loginsByAccountId.GetValueOrDefault(user.Id)))
            .ToList();
    }

    public async Task LeaveFamily(long telegramId)
    {
        var user = await FindTelegramUserAsync(telegramId);
        if (user is null)
        {
            return;
        }

        user.LeaveFamily();
        await _userRepository.UpdateAsync(user);
    }

    public async Task SetBalanceAsync(long telegramId, decimal balance)
    {
        var user = await FindTelegramUserAsync(telegramId)
            ?? throw new InvalidOperationException("User not found.");

        var result = user.SetBalance(balance);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error);
        }

        await _userRepository.UpdateAsync(user);
    }

    public async Task UpdateTelegramUsernameAsync(
        long telegramId,
        string? telegramUsername)
    {
        var user = await FindTelegramUserAsync(telegramId);
        if (user is null)
        {
            return;
        }

        var providerSubject = ToTelegramSubject(telegramId);
        var login = await _externalLoginRepository.GetAsync(
            ExternalLoginProvider.Telegram,
            providerSubject);

        if (login is null)
        {
            return;
        }

        var result = login.Touch(DateTimeOffset.UtcNow, telegramUsername);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error);
        }

        await _externalLoginRepository.UpdateAsync(login);
    }

    private Task<User?> FindTelegramUserAsync(long telegramId) =>
        _userRepository.GetByExternalLoginAsync(
            ExternalLoginProvider.Telegram,
            ToTelegramSubject(telegramId));

    private static UserDto ToDto(User user, ExternalLogin? telegramLogin)
    {
        long? telegramId = null;
        if (telegramLogin is not null &&
            long.TryParse(
                telegramLogin.ProviderSubject,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var parsedTelegramId))
        {
            telegramId = parsedTelegramId;
        }

        return new UserDto(
            user.Id,
            user.FamilyId,
            user.Name,
            telegramId,
            user.Balance,
            telegramLogin?.ProviderUsername);
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

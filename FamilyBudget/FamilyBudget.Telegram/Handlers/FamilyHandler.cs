using FamilyBudget.Application.Interfaces;
using FamilyBudget.Telegram.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;

public class FamilyHandler
{
    private readonly IUserService _userService;
    private readonly RegistrationHandler _registrationHandler;
    public FamilyHandler(IUserService userService, RegistrationHandler registrationHandler)
    {
        _userService = userService;
        _registrationHandler = registrationHandler;
    }

    public async Task ShowMembers(
        ITelegramBotClient bot,
        CallbackQuery query)
    {
        var chatId = query.Message?.Chat.Id ?? query.From.Id;

        var user = await _userService.GetByTelegramIdAsync(query.From!.Id);

        if (user == null)
        {
            await bot.SendMessage(
                chatId,
                "❌ Пользователь не найден");
            return;
        }

        var members = await _userService.GetByFamilyIdAsync(user.FamilyId);

        if (members.Count == 0)
        {
            await bot.SendMessage(
                chatId,
                "В семье пока нет участников");
            return;
        }

        var text = "👨‍👩‍👧 Участники семьи\n\n";

        int i = 1;

        foreach (var member in members)
        {
            text += $"{i}. @{member.Name}\n";
            i++;
        }

        await bot.SendMessage(chatId, text);
    }
    public async Task LeaveFamily(
        ITelegramBotClient bot,
        CallbackQuery query)
    {
        var chatId = query.Message?.Chat.Id ?? query.From.Id;
        await _userService.LeaveFamily(query.From!.Id);

        await bot.SendMessage(
            chatId,
            "Вы покинули семью.\nСоздадим новую.");

        // запускаем тот же сценарий регистрации
        await _registrationHandler.StartAsync(
            bot,
            new CallbackQuery
            {
                From = query.From,
                Message = query.Message
            });
    }
}
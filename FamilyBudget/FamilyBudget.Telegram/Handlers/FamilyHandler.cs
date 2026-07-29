using FamilyBudget.Application.Interfaces;
using FamilyBudget.Telegram.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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

        if (user == null || !user.FamilyId.HasValue)
        {
            await bot.SendMessage(
                chatId,
                "❌ Пользователь не найден");
            return;
        }

        var members = await _userService.GetByFamilyIdAsync(user.FamilyId.Value);

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
            var telegramUsername = string.IsNullOrWhiteSpace(member.TelegramUsername)
                ? string.Empty
                : $" (@{member.TelegramUsername})";
            text += $"{i}. {member.Name}{telegramUsername}\n";
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

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Создать семью", "create_family"),
                InlineKeyboardButton.WithCallbackData("Войти по коду", "enter_invite_code")
            }
        });

        await bot.SendMessage(
            chatId,
            "Вы покинули семью. Создайте новую или введите пригласительный код.",
            replyMarkup: keyboard);
    }
}

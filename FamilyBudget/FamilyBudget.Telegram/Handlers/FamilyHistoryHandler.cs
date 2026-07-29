using FamilyBudget.Application.Interfaces;
using FamilyBudget.Telegram.History;
using FamilyBudget.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Telegram.Handlers;

public sealed class FamilyHistoryHandler
{
    private const int PageSize = 8;

    private readonly IFamilyHistoryService _historyService;
    private readonly IUserService _userService;

    public FamilyHistoryHandler(
        IFamilyHistoryService historyService,
        IUserService userService)
    {
        _historyService = historyService;
        _userService = userService;
    }

    public async Task ShowAsync(
        ITelegramBotClient bot,
        CallbackQuery query,
        int page = 1)
    {
        if (query.Message is null)
            return;

        var user = await _userService.GetByTelegramIdAsync(query.From.Id);
        if (user is null || !user.FamilyId.HasValue)
        {
            await bot.SendMessage(
                query.Message.Chat.Id,
                "Вы не зарегистрированы или не состоите в семье.");
            return;
        }

        var history = await _historyService.GetFamilyHistoryAsync(
            user.FamilyId.Value,
            page,
            PageSize);

        await bot.EditMessageText(
            query.Message.Chat.Id,
            query.Message.MessageId,
            FamilyHistoryMessageFormatter.Format(history),
            replyMarkup: FamilyHistoryKeyboard.Create(history));
    }
}

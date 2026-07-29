using FamilyBudget.Application.Interfaces;
using FamilyBudget.Telegram.Keyboards;
using FamilyBudget.Telegram.Receipts;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Telegram.Handlers;

public sealed class ReceiptHistoryHandler
{
    private const int ReceiptPageSize = 5;
    private const int ItemPageSize = 8;

    private readonly IReceiptService _receiptService;
    private readonly IUserService _userService;

    public ReceiptHistoryHandler(
        IReceiptService receiptService,
        IUserService userService)
    {
        _receiptService = receiptService;
        _userService = userService;
    }

    public async Task ShowListAsync(
        ITelegramBotClient bot,
        CallbackQuery query,
        int page = 1)
    {
        if (query.Message is null)
            return;

        var familyId = await GetFamilyIdAsync(query.From.Id);
        if (!familyId.HasValue)
        {
            await bot.SendMessage(
                query.Message.Chat.Id,
                "Вы не зарегистрированы или не состоите в семье.");
            return;
        }

        var history = await _receiptService.GetFamilyPageAsync(
            familyId.Value,
            page,
            ReceiptPageSize);

        await bot.EditMessageText(
            query.Message.Chat.Id,
            query.Message.MessageId,
            ReceiptHistoryMessageFormatter.FormatList(history),
            replyMarkup: ReceiptHistoryKeyboard.CreateList(history));
    }

    public async Task ShowDetailsAsync(
        ITelegramBotClient bot,
        CallbackQuery query,
        Guid receiptId,
        int itemPage,
        ReceiptNavigationOrigin origin,
        int originPage)
    {
        if (query.Message is null)
            return;

        var familyId = await GetFamilyIdAsync(query.From.Id);
        if (!familyId.HasValue)
        {
            await bot.SendMessage(
                query.Message.Chat.Id,
                "Вы не зарегистрированы или не состоите в семье.");
            return;
        }

        var receipt = await _receiptService.GetByIdForFamilyAsync(receiptId, familyId.Value);
        if (receipt is null)
        {
            await bot.EditMessageText(
                query.Message.Chat.Id,
                query.Message.MessageId,
                "Чек не найден или недоступен.",
                replyMarkup: ReceiptHistoryKeyboard.CreateDetails(
                    receiptId,
                    1,
                    1,
                    origin,
                    Math.Max(1, originPage)));
            return;
        }

        var totalItemPages = ReceiptHistoryMessageFormatter.GetItemPageCount(receipt, ItemPageSize);
        var actualItemPage = Math.Clamp(itemPage, 1, totalItemPages);

        await bot.EditMessageText(
            query.Message.Chat.Id,
            query.Message.MessageId,
            ReceiptHistoryMessageFormatter.FormatDetails(receipt, actualItemPage, ItemPageSize),
            replyMarkup: ReceiptHistoryKeyboard.CreateDetails(
                receipt.Id,
                actualItemPage,
                totalItemPages,
                origin,
                Math.Max(1, originPage)));
    }

    private async Task<Guid?> GetFamilyIdAsync(long telegramId)
    {
        var user = await _userService.GetByTelegramIdAsync(telegramId);
        return user?.FamilyId;
    }
}

using FamilyBudget.Application.Dtos.Receipt;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Enums;
using FamilyBudget.Telegram.Keyboards;
using FamilyBudget.Telegram.Receipts;
using FamilyBudget.Telegram.State;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Telegram.Handlers;

public class ReceiptHandler
{
    private readonly IReceiptService _receiptService;
    private readonly IUserService _userService;
    private readonly UserStateService _state;
    private readonly ReceiptProcessingQueue _queue;
    private readonly ILogger<ReceiptHandler> _logger;

    public ReceiptHandler(
        IReceiptService receiptService,
        IUserService userService,
        UserStateService state,
        ReceiptProcessingQueue queue,
        ILogger<ReceiptHandler> logger)
    {
        _receiptService = receiptService;
        _userService = userService;
        _state = state;
        _queue = queue;
        _logger = logger;
    }

    public async Task StartAsync(ITelegramBotClient bot, CallbackQuery query)
    {
        _state.Set(query.From.Id, UserState.WaitingForReceiptPhoto);
        await bot.SendMessage(
            query.Message!.Chat.Id,
            "📷 Отправьте фотографию чека. После распознавания выберите категорию, чтобы подтвердить расход.");
    }

    public async Task HandlePhotoAsync(
        ITelegramBotClient bot,
        Message message,
        CancellationToken cancellationToken)
    {
        var telegramUserId = message.From?.Id;
        if (telegramUserId is null)
            return;

        if (_state.Get(telegramUserId.Value) != UserState.WaitingForReceiptPhoto)
        {
            await bot.SendMessage(
                message.Chat.Id,
                "Для добавления чека выберите «➕ Расход» → «📷 По фото чека».",
                cancellationToken: cancellationToken);
            return;
        }

        var user = await _userService.GetByTelegramIdAsync(telegramUserId.Value);
        if (user?.FamilyId is null)
        {
            _state.Clear(telegramUserId.Value);
            await bot.SendMessage(
                message.Chat.Id,
                "Сначала зарегистрируйтесь и присоединитесь к семье.",
                cancellationToken: cancellationToken);
            return;
        }

        try
        {
            await bot.SendMessage(
                message.Chat.Id,
                "📥 Загружаю фотографию чека…",
                cancellationToken: cancellationToken);

            var photo = message.Photo![^1];
            var file = await bot.GetFile(photo.FileId, cancellationToken);
            if (string.IsNullOrWhiteSpace(file.FilePath))
                throw new InvalidOperationException("Telegram did not return the receipt file path.");

            await using var image = new MemoryStream();
            await bot.DownloadFile(file.FilePath, image, cancellationToken);

            var job = new ReceiptProcessingJob(
                message.Chat.Id,
                new CreateReceiptDto(
                    user.FamilyId.Value,
                    user.Id,
                    photo.FileId,
                    image.ToArray(),
                    "image/jpeg"));

            if (!_queue.TryEnqueue(job))
            {
                await bot.SendMessage(
                    message.Chat.Id,
                    "Очередь распознавания заполнена. Попробуйте отправить чек немного позже.",
                    cancellationToken: cancellationToken);
                return;
            }

            _state.Clear(telegramUserId.Value);
            await bot.SendMessage(
                message.Chat.Id,
                "✅ Фото принято. Чек распознаётся в фоне — пока можно пользоваться другими командами бота.",
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Telegram polling остановлен вместе с приложением.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue receipt for Telegram user {TelegramUserId}", telegramUserId);
            await bot.SendMessage(
                message.Chat.Id,
                "Не удалось принять фотографию чека. Попробуйте отправить её ещё раз.",
                cancellationToken: cancellationToken);
        }
    }

    public async Task ConfirmAsync(ITelegramBotClient bot, CallbackQuery query)
    {
        var parts = query.Data!.Split(':');
        if (parts.Length != 3 || !Guid.TryParse(parts[1], out var receiptId) ||
            !int.TryParse(parts[2], out var categoryValue) ||
            !Enum.IsDefined(typeof(ExpenseCategory), categoryValue))
        {
            await bot.SendMessage(query.Message!.Chat.Id, "Некорректные данные чека.");
            return;
        }

        try
        {
            await _receiptService.ConfirmAndCreateExpenseAsync(receiptId, (ExpenseCategory)categoryValue);
            await bot.SendMessage(
                query.Message!.Chat.Id,
                "✅ Чек подтверждён, расход добавлен.",
                replyMarkup: KeyboardFactory.MainMenu());
        }
        catch (InvalidOperationException ex) when (ex.Message == "Insufficient funds.")
        {
            await bot.SendMessage(query.Message!.Chat.Id, "Недостаточно средств для добавления расхода.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Receipt {ReceiptId} confirmation failed", receiptId);
            await bot.SendMessage(query.Message!.Chat.Id, "Не удалось подтвердить чек.");
        }
    }

    public async Task RejectAsync(ITelegramBotClient bot, CallbackQuery query)
    {
        var receiptIdText = query.Data!["receipt_reject:".Length..];
        if (!Guid.TryParse(receiptIdText, out var receiptId))
        {
            await bot.SendMessage(query.Message!.Chat.Id, "Некорректные данные чека.");
            return;
        }

        try
        {
            await _receiptService.RejectAsync(receiptId);
            await bot.SendMessage(
                query.Message!.Chat.Id,
                "Чек не будет учтён в расходах.",
                replyMarkup: KeyboardFactory.MainMenu());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Receipt {ReceiptId} rejection failed", receiptId);
            await bot.SendMessage(query.Message!.Chat.Id, "Не удалось отклонить чек.");
        }
    }
}

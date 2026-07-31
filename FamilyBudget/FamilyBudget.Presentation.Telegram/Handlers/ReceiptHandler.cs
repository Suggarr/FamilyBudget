using FamilyBudget.Application.Dtos.Receipt;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Enums;
using FamilyBudget.Presentation.Telegram.Keyboards;
using FamilyBudget.Presentation.Telegram.Receipts;
using FamilyBudget.Presentation.Telegram.State;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Presentation.Telegram.Handlers;

public class ReceiptHandler
{
    private const int MaxReceiptImageBytes = 20 * 1024 * 1024;

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
            "📷 Отправьте фотографию чека или прикрепите изображение как файл без сжатия (JPG, PNG или WebP). " +
            "После распознавания выберите категорию, чтобы подтвердить расход.");
    }

    public async Task HandlePhotoAsync(
        ITelegramBotClient bot,
        Message message,
        CancellationToken cancellationToken)
    {
        var photo = message.Photo![^1];
        await HandleImageAsync(bot, message, photo.FileId, cancellationToken);
    }

    public async Task HandleDocumentAsync(
        ITelegramBotClient bot,
        Message message,
        CancellationToken cancellationToken)
    {
        var document = message.Document!;
        await HandleImageAsync(bot, message, document.FileId, cancellationToken);
    }

    private async Task HandleImageAsync(
        ITelegramBotClient bot,
        Message message,
        string fileId,
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
                "📥 Загружаю изображение чека…",
                cancellationToken: cancellationToken);

            var file = await bot.GetFile(fileId, cancellationToken);
            if (string.IsNullOrWhiteSpace(file.FilePath))
                throw new InvalidOperationException("Telegram did not return the receipt file path.");

            await using var image = new MemoryStream();
            await bot.DownloadFile(file.FilePath, image, cancellationToken);

            if (image.Length == 0)
                throw new InvalidOperationException("Telegram returned an empty receipt image.");

            if (image.Length > MaxReceiptImageBytes)
            {
                await bot.SendMessage(
                    message.Chat.Id,
                    "Файл слишком большой. Максимальный размер изображения чека — 20 МБ.",
                    cancellationToken: cancellationToken);
                return;
            }

            var imageBytes = image.ToArray();
            var mediaType = DetectImageMediaType(imageBytes);
            if (mediaType is null)
            {
                await bot.SendMessage(
                    message.Chat.Id,
                    "Этот файл не является поддерживаемым изображением. Отправьте чек в формате JPG, PNG или WebP.",
                    cancellationToken: cancellationToken);
                return;
            }

            var job = new ReceiptProcessingJob(
                message.Chat.Id,
                new CreateReceiptDto(
                    user.FamilyId.Value,
                    user.Id,
                    fileId,
                    imageBytes,
                    mediaType));

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
                "✅ Изображение принято. Чек распознаётся в фоне — пока можно пользоваться другими командами бота.",
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

    private static string? DetectImageMediaType(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return "image/jpeg";

        if (bytes.Length >= 8 &&
            bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
            bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
            return "image/png";

        if (bytes.Length >= 12 &&
            bytes[0] == (byte)'R' && bytes[1] == (byte)'I' && bytes[2] == (byte)'F' && bytes[3] == (byte)'F' &&
            bytes[8] == (byte)'W' && bytes[9] == (byte)'E' && bytes[10] == (byte)'B' && bytes[11] == (byte)'P')
            return "image/webp";

        return null;
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

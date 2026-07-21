using FamilyBudget.Application.Interfaces;
using FamilyBudget.Telegram.Keyboards;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Telegram.Bot;

namespace FamilyBudget.Telegram.Receipts;

public sealed class ReceiptProcessingWorker : BackgroundService
{
    private readonly ReceiptProcessingQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITelegramBotClient _bot;
    private readonly ILogger<ReceiptProcessingWorker> _logger;

    public ReceiptProcessingWorker(
        ReceiptProcessingQueue queue,
        IServiceScopeFactory scopeFactory,
        ITelegramBotClient bot,
        ILogger<ReceiptProcessingWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _bot = bot;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _queue.ReadAllAsync(stoppingToken))
        {
            await ProcessAsync(job, stoppingToken);
        }
    }

    private async Task ProcessAsync(ReceiptProcessingJob job, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var receiptService = scope.ServiceProvider.GetRequiredService<IReceiptService>();
            var receipt = await receiptService.ParseAndSaveAsync(job.Receipt, cancellationToken);

            await _bot.SendMessage(
                job.ChatId,
                ReceiptMessageFormatter.Format(receipt),
                replyMarkup: KeyboardFactory.ReceiptCategories(receipt.Id),
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Нормальное завершение приложения: не отправляем пользователю ложную ошибку.
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Receipt processing timed out for chat {ChatId}", job.ChatId);
            await NotifyFailureAsync(
                job.ChatId,
                "Распознавание чека заняло слишком много времени. Попробуйте отправить изображение ещё раз.",
                cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Receipt recognition returned incomplete JSON for chat {ChatId}", job.ChatId);
            await NotifyFailureAsync(
                job.ChatId,
                "Модель не смогла сформировать полный результат распознавания. Попробуйте отправить чек ещё раз.",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Receipt processing failed for chat {ChatId}", job.ChatId);

            await NotifyFailureAsync(
                job.ChatId,
                "Не удалось обработать чек. Попробуйте отправить изображение ещё раз.",
                cancellationToken);
        }
    }

    private async Task NotifyFailureAsync(long chatId, string message, CancellationToken cancellationToken)
    {
        try
        {
            await _bot.SendMessage(chatId, message, cancellationToken: cancellationToken);
        }
        catch (Exception notificationException)
        {
            _logger.LogError(
                notificationException,
                "Failed to notify chat {ChatId} about receipt processing error",
                chatId);
        }
    }
}

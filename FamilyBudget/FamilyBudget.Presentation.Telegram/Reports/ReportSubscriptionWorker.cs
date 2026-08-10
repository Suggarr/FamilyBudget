using FamilyBudget.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace FamilyBudget.Presentation.Telegram.Reports;

public sealed class ReportSubscriptionWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);
    private const int TelegramMessageLimit = 4000;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITelegramBotClient _bot;
    private readonly ILogger<ReportSubscriptionWorker> _logger;

    public ReportSubscriptionWorker(
        IServiceScopeFactory scopeFactory,
        ITelegramBotClient bot,
        ILogger<ReportSubscriptionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _bot = bot;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueSubscriptionsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to process due report subscriptions");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessDueSubscriptionsAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var subscriptionService = scope.ServiceProvider.GetRequiredService<IReportSubscriptionService>();
        var dueSubscriptions = await subscriptionService.GetDueAsync(DateTime.UtcNow);

        foreach (var subscription in dueSubscriptions)
        {
            try
            {
                await ProcessSubscriptionAsync(scope.ServiceProvider, subscription, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to send scheduled report for subscription {SubscriptionId}",
                    subscription.Id);
            }
        }
    }

    private async Task ProcessSubscriptionAsync(
        IServiceProvider serviceProvider,
        Application.Dtos.ReportSubscription.ReportSubscriptionDto subscription,
        CancellationToken cancellationToken)
    {
        var userService = serviceProvider.GetRequiredService<IUserService>();
        var reportService = serviceProvider.GetRequiredService<IFamilyReportService>();
        var subscriptionService = serviceProvider.GetRequiredService<IReportSubscriptionService>();

        var user = await userService.GetByIdAsync(subscription.UserId);
        if (user is null || !user.FamilyId.HasValue || !user.TelegramId.HasValue)
        {
            await subscriptionService.DisableAsync(subscription.UserId);
            _logger.LogWarning(
                "Disabled report subscription {SubscriptionId}: user is missing or has no family",
                subscription.Id);
            return;
        }

        var utcNow = DateTime.UtcNow;
        var reportEnd = GetLatestScheduledRun(subscription.NextRunAt, utcNow);
        var reportStart = reportEnd.AddDays(-7);
        var report = await reportService.GetFamilyReportAsync(
            user.FamilyId.Value,
            reportStart,
            reportEnd);
        var message = FamilyReportMessageFormatter.Format(
            report,
            $"за неделю до {reportEnd:dd.MM.yyyy HH:mm} UTC");

        await SendInChunksAsync(user.TelegramId.Value, message, cancellationToken);
        await subscriptionService.ScheduleNextRunAsync(subscription.UserId, DateTime.UtcNow);

        _logger.LogInformation(
            "Scheduled report {SubscriptionId} sent to user {UserId}",
            subscription.Id,
            subscription.UserId);
    }

    private async Task SendInChunksAsync(
        long chatId,
        string message,
        CancellationToken cancellationToken)
    {
        var remainingMessage = message;

        while (remainingMessage.Length > TelegramMessageLimit)
        {
            var splitIndex = remainingMessage.LastIndexOf('\n', TelegramMessageLimit);
            if (splitIndex <= 0)
                splitIndex = TelegramMessageLimit;

            await _bot.SendMessage(
                chatId,
                remainingMessage[..splitIndex],
                cancellationToken: cancellationToken);

            remainingMessage = remainingMessage[splitIndex..].TrimStart('\r', '\n');
        }

        if (remainingMessage.Length > 0)
            await _bot.SendMessage(chatId, remainingMessage, cancellationToken: cancellationToken);
    }

    private static DateTime GetLatestScheduledRun(DateTime nextRunAt, DateTime utcNow)
    {
        while (nextRunAt.AddDays(7) <= utcNow)
            nextRunAt = nextRunAt.AddDays(7);

        return nextRunAt;
    }
}

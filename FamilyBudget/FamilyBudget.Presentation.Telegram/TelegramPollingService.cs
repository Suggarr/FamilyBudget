using FamilyBudget.Presentation.Telegram.Bot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace FamilyBudget.Presentation.Telegram;

internal sealed class TelegramPollingService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramPollingService> _logger;

    public TelegramPollingService(
        ITelegramBotClient bot,
        IServiceScopeFactory scopeFactory,
        ILogger<TelegramPollingService> logger)
    {
        _bot = bot;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Telegram bot polling started");

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal host shutdown.
        }
    }

    private async Task HandleUpdateAsync(
        ITelegramBotClient bot,
        Update update,
        CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<BotHandler>();
        await handler.HandleUpdateAsync(bot, update, cancellationToken);
    }

    private async Task HandleErrorAsync(
        ITelegramBotClient bot,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Telegram polling error from {Source}",
            source);

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal host shutdown.
        }
    }
}

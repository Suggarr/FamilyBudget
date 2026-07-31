using FamilyBudget.Presentation.Telegram.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FamilyBudget.Presentation.Telegram.Bot;

public class BotHandler
{
    private readonly CommandRouter _commandRouter;
    private readonly CallbackRouter _callbackRouter;
    private readonly MessageRouter _messageRouter;
    private readonly ReportHandler _reportHandler;
    private readonly InviteHandler _inviteHandler;
    private readonly ReceiptHandler _receiptHandler;

    public BotHandler(
    CommandRouter commandRouter,
    CallbackRouter callbackRouter,
    MessageRouter messageRouter,
    ReportHandler reportHandler,
    InviteHandler inviteHandler,
    ReceiptHandler receiptHandler)
    {
        _commandRouter = commandRouter;
        _callbackRouter = callbackRouter;
        _messageRouter = messageRouter;
        _reportHandler = reportHandler;
        _inviteHandler = inviteHandler;
        _receiptHandler = receiptHandler;
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient bot,
        Update update,
        CancellationToken ct = default!)
    {
        Console.WriteLine($"Update received: {update.Type}");

        if (update.Type == UpdateType.Message && update.Message!.Photo is { Length: > 0 })
        {
            await _receiptHandler.HandlePhotoAsync(bot, update.Message, ct);
            return;
        }

        if (update.Type == UpdateType.Message && update.Message!.Document is not null)
        {
            await _receiptHandler.HandleDocumentAsync(bot, update.Message, ct);
            return;
        }

        if (update.Type == UpdateType.Message && update.Message!.Text != null)
        {
            if (update.Message.Text.StartsWith("/start join_"))
            {
                await _inviteHandler.JoinByInvite(bot, update.Message);
                return;
            }

            if (update.Message.Text.StartsWith("/"))
            {
                await _commandRouter.RouteAsync(bot, update.Message);
            }
            else
            {
                await _messageRouter.RouteAsync(bot, update.Message);
            }
        }

        if (update.Type == UpdateType.CallbackQuery)
            await _callbackRouter.RouteAsync(bot, update.CallbackQuery!);

    }

    public Task HandleErrorAsync(
        ITelegramBotClient bot,
        Exception ex,
        CancellationToken ct)
    {
        Console.Error.WriteLine($"Telegram polling error: {ex}");
        return Task.CompletedTask;
    }
}

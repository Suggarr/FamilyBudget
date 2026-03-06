using FamilyBudget.Telegram.Bot;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class BotHandler
{
    private readonly CommandRouter _commandRouter;
    private readonly CallbackRouter _callbackRouter;
    private readonly MessageRouter _messageRouter;

    public BotHandler(
        CommandRouter commandRouter,
        CallbackRouter callbackRouter,
        MessageRouter messageRouter)
    {
        _commandRouter = commandRouter;
        _callbackRouter = callbackRouter;
        _messageRouter = messageRouter;
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient bot,
        Update update,
        CancellationToken ct = default!)
    {
        Console.WriteLine($"Update received: {update.Type}");

        if (update.Type == UpdateType.Message && update.Message!.Text != null)
        {
            if (update.Message.Text.StartsWith("/"))
                await _commandRouter.RouteAsync(bot, update.Message);
            else
                await _messageRouter.RouteAsync(bot, update.Message);
        }

        if (update.Type == UpdateType.CallbackQuery)
            await _callbackRouter.RouteAsync(bot, update.CallbackQuery!);
    }

    public Task HandleErrorAsync(
        ITelegramBotClient bot,
        Exception ex,
        CancellationToken ct)
    {
        Console.WriteLine(ex.Message);
        return Task.CompletedTask;
    }
}
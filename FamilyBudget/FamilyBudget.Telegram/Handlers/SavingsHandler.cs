using System.Net;
using System.Text;
using FamilyBudget.Application.Dtos.Savings;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Telegram.Keyboards;
using FamilyBudget.Telegram.State;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FamilyBudget.Telegram.Handlers;

public class SavingsHandler
{
    private readonly ISavingsService _savingsService;
    private readonly IUserService _userService;
    private readonly UserStateService _state;

    public SavingsHandler(ISavingsService savingsService, IUserService userService, UserStateService state)
    {
        _savingsService = savingsService;
        _userService = userService;
        _state = state;
    }

    public async Task ShowAsync(ITelegramBotClient bot, CallbackQuery query)
    {
        var user = await _userService.GetByTelegramIdAsync(query.From.Id);
        if (user?.FamilyId is null)
        {
            await bot.SendMessage(query.Message!.Chat.Id, "Вы не состоите в семье.");
            return;
        }

        var summary = await _savingsService.GetSummaryAsync(user.FamilyId.Value);
        var text = new StringBuilder($"🏦 <b>Семейная копилка: {summary.TotalAmount:F2}</b>\n");

        if (summary.Operations.Count == 0)
        {
            text.Append("\nОпераций пока нет.");
        }
        else
        {
            text.Append("\nПоследние операции:\n");
            foreach (var item in summary.Operations.Take(10))
            {
                var sign = item.Type == SavingsOperationType.Contribution ? "+" : "−";
                var action = item.Type == SavingsOperationType.Contribution ? "вклад" : "снятие";
                text.Append($"\n• {WebUtility.HtmlEncode(item.UserName)}: {sign}{item.Amount:F2} ({action}) — {item.CreatedAt.ToLocalTime():dd.MM.yyyy}");
            }
        }

        await bot.SendMessage(
            query.Message!.Chat.Id,
            text.ToString(),
            parseMode: ParseMode.Html,
            replyMarkup: KeyboardFactory.SavingsMenu());
    }

    public async Task StartContributionAsync(ITelegramBotClient bot, CallbackQuery query)
    {
        _state.Set(query.From.Id, UserState.WaitingForSavingsContribution);
        await bot.SendMessage(query.Message!.Chat.Id, "Введите сумму вклада в семейную копилку:");
    }

    public async Task StartWithdrawalAsync(ITelegramBotClient bot, CallbackQuery query)
    {
        _state.Set(query.From.Id, UserState.WaitingForSavingsWithdrawal);
        await bot.SendMessage(query.Message!.Chat.Id, "Введите сумму, которую хотите снять из копилки:");
    }

    public Task HandleContributionAsync(ITelegramBotClient bot, Message message) =>
        HandleAmountAsync(bot, message, isWithdrawal: false);

    public Task HandleWithdrawalAsync(ITelegramBotClient bot, Message message) =>
        HandleAmountAsync(bot, message, isWithdrawal: true);

    private async Task HandleAmountAsync(ITelegramBotClient bot, Message message, bool isWithdrawal)
    {
        if (!decimal.TryParse(message.Text, out var amount) || amount <= 0)
        {
            await bot.SendMessage(message.Chat.Id, "Введите корректную положительную сумму.");
            return;
        }

        var user = await _userService.GetByTelegramIdAsync(message.From!.Id);
        if (user?.FamilyId is null)
        {
            _state.Clear(message.From.Id);
            await bot.SendMessage(message.Chat.Id, "Вы не состоите в семье.");
            return;
        }

        try
        {
            if (isWithdrawal)
                await _savingsService.WithdrawAsync(user.FamilyId.Value, user.Id, amount);
            else
                await _savingsService.ContributeAsync(user.FamilyId.Value, user.Id, amount);
        }
        catch (InvalidOperationException ex) when (ex.Message == "Insufficient funds.")
        {
            await bot.SendMessage(message.Chat.Id, $"Недостаточно средств. На вашем счёте: {user.Balance:F2}");
            return;
        }
        catch (InvalidOperationException ex) when (ex.Message == "Insufficient savings.")
        {
            var summary = await _savingsService.GetSummaryAsync(user.FamilyId.Value);
            await bot.SendMessage(message.Chat.Id, $"Недостаточно средств в копилке. Доступно: {summary.TotalAmount:F2}");
            return;
        }

        _state.Clear(message.From.Id);
        var resultText = isWithdrawal
            ? $"✅ Из копилки снято <b>{amount:F2}</b> и зачислено на ваш счёт."
            : $"✅ В копилку добавлено <b>{amount:F2}</b>.";

        await bot.SendMessage(
            message.Chat.Id,
            resultText,
            parseMode: ParseMode.Html,
            replyMarkup: KeyboardFactory.MainMenu());
    }
}

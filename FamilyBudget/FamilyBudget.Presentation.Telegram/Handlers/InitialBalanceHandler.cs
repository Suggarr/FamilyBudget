using FamilyBudget.Application.Dtos.Income;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Presentation.Telegram.State;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyBudget.Presentation.Telegram.Handlers
{
    public class InitialBalanceHandler
    {
        private readonly IUserService _userService;
        private readonly UserStateService _state;

        public InitialBalanceHandler(
            IUserService userService,
            UserStateService state)
        {
            _userService = userService;
            _state = state;
        }

        public async Task ShowAsync(ITelegramBotClient bot, CallbackQuery query)
        {
            var user = await _userService.GetByTelegramIdAsync(query.From.Id);
            if (user is null)
            {
                await bot.SendMessage(query.Message!.Chat.Id, "Пользователь не найден.");
                return;
            }

            await bot.SendMessage(
                query.Message!.Chat.Id,
                $"💳 На вашем счёте: <b>{user.Balance:F2}</b>",
                parseMode: ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("Указать баланс", "set_account_balance") },
                    new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", "main_menu") }
                }));
        }

        public async Task StartAsync(
            ITelegramBotClient bot,
            CallbackQuery query)
        {
            _state.Set(query.From.Id, UserState.WaitingForInitialBalance);

            await bot.SendMessage(
                query.Message!.Chat.Id,
                "Введите текущий баланс на вашем счёте:");
        }

        public async Task HandleBalanceAsync(
            ITelegramBotClient bot,
            Message message)
        {
            if (!decimal.TryParse(message.Text, out var amount))
            {
                await bot.SendMessage(
                    message.Chat.Id,
                    "Введите корректную сумму.");
                return;
            }

            if (amount < 0)
            {
                await bot.SendMessage(
                    message.Chat.Id,
                    "Баланс не может быть отрицательным.");
                return;
            }

            var user = await _userService.GetByTelegramIdAsync(message.From!.Id);

            if (user is null)
            {
                await bot.SendMessage(message.Chat.Id, "Пользователь не найден.");
                return;
            }

            await _userService.SetBalanceAsync(message.From.Id, amount);

            _state.Clear(message.From.Id);

            await bot.SendMessage(
                message.Chat.Id,
                $"✅ Баланс счёта установлен: <b>{amount:F2}</b>",
                parseMode: ParseMode.Html,
                replyMarkup: Keyboards.KeyboardFactory.MainMenu());
        }
    }
}


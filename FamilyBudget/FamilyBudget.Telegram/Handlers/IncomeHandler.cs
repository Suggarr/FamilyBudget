using FamilyBudget.Application.Dtos.Income;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Telegram.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Telegram.Handlers
{
    public class IncomeHandler
    {
        private readonly IIncomeService _incomeService;
        private readonly IUserService _userService;
        private readonly UserStateService _state;
        private readonly TempIncomeStorage _storage;

        public IncomeHandler(
            IIncomeService incomeService,
            IUserService userService,
            UserStateService state,
            TempIncomeStorage storage)
        {
            _incomeService = incomeService;
            _userService = userService;
            _state = state;
            _storage = storage;
        }

        public async Task StartAsync(
            ITelegramBotClient bot,
            CallbackQuery query)
        {
            _state.Set(query.From.Id, UserState.WaitingForIncomeAmount);

            await bot.SendMessage(
                query.Message!.Chat.Id,
                "Введите сумму дохода:");
        }

        public async Task HandleAmountAsync(
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

            _storage.SetAmount(message.From!.Id, amount);

            _state.Set(message.From.Id, UserState.WaitingForIncomeDescription);

            await bot.SendMessage(
                message.Chat.Id,
                "Введите описание дохода:");
        }

        public async Task HandleDescriptionAsync(
            ITelegramBotClient bot,
            Message message)
        {
            var amount = _storage.GetAmount(message.From!.Id);
            var description = message.Text ?? "";

            var user = await _userService.GetByTelegramIdAsync(message.From.Id);

            if (user is null)
            {
                await bot.SendMessage(message.Chat.Id, "Пользователь не найден.");
                return;
            }

            var dto = new CreateIncomeDto(
                user.FamilyId,
                user.Id,
                amount,
                description,
                DateTime.UtcNow);

            await _incomeService.AddAsync(dto);

            _storage.Clear(message.From.Id);
            _state.Clear(message.From.Id);

            await bot.SendMessage(
                message.Chat.Id,
                "💵 Доход добавлен!");
        }
    }
}

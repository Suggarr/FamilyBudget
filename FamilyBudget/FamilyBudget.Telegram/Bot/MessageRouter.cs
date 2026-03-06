//using FamilyBudget.Application.Services;
//using FamilyBudget.Core.Dtos.Expense;
//using FamilyBudget.Telegram.Handlers;
//using FamilyBudget.Telegram.Keyboards;
//using FamilyBudget.Telegram.State;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Telegram.Bot;
//using Telegram.Bot.Types;
//namespace FamilyBudget.Telegram.Bot
//{
//    public class MessageRouter
//    {
//        private readonly UserStateService _state;
//        private readonly TempExpenseStorage _storage;
//        private readonly IExpenseService _expenseService;
//        private readonly RegistrationHandler _registrationHandler;

//        public MessageRouter(UserStateService state, TempExpenseStorage storage, IExpenseService expenseService,
//            RegistrationHandler registrationHandler)
//        {
//            _state = state;
//            _storage = storage;
//            _expenseService = expenseService;
//            _registrationHandler = registrationHandler;
//        }

//        public async Task RouteAsync(ITelegramBotClient bot, Message message)
//        {
//            await _registrationHandler.HandleMessageAsync(bot, message);
//            var userState = _state.Get(message.From!.Id);
//            if (userState == UserState.WaitingForExpenseAmount)
//            {
//                if (!decimal.TryParse(message.Text, out var amount))
//                {
//                    await bot.SendMessage(message.Chat.Id, "Введите корректную сумму");
//                    return;
//                }
//                _storage.SaveAmount(message.From.Id, amount);
//                _state.Clear(message.From.Id);

//                Guid familyId = Guid.Empty;

//                var dto = new CreateExpenseDto(familyId, Guid.Empty, Guid.Empty, amount, "", DateTime.UtcNow);

//                await _expenseService.AddAsync(dto);
//                await bot.SendMessage(message.Chat.Id, "Расход добавлен!", replyMarkup: KeyboardFactory.MainMenu());
//            }
//        }
//    }
//}
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using FamilyBudget.Core.Dtos.Expense;
using FamilyBudget.Telegram.Handlers;
using FamilyBudget.Telegram.Keyboards;
using FamilyBudget.Telegram.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Telegram.Bot
{
    public class MessageRouter
    {
        private readonly UserStateService _state;
        private readonly TempExpenseStorage _storage;
        private readonly IExpenseService _expenseService;
        private readonly RegistrationHandler _registrationHandler;
        private readonly IUserService _userService; 

        public MessageRouter(
            UserStateService state,
            TempExpenseStorage storage,
            IExpenseService expenseService,
            RegistrationHandler registrationHandler,
            IUserService userService)
        {
            _state = state;
            _storage = storage;
            _expenseService = expenseService;
            _registrationHandler = registrationHandler;
            _userService = userService;
        }

        public async Task RouteAsync(
            ITelegramBotClient bot,
            Message message)
        {
            await _registrationHandler.HandleMessageAsync(bot, message);

            var userId = message.From!.Id;
            var userState = _state.Get(userId);

            // ===== ВВОД СУММЫ =====
            if (userState == UserState.WaitingForExpenseAmount)
            {
                if (!decimal.TryParse(message.Text, out var amount))
                {
                    await bot.SendMessage(
                        message.Chat.Id,
                        "Введите корректную сумму");
                    return;
                }

                _storage.SaveAmount(userId, amount);

                // следующий шаг
                _state.Set(userId, UserState.WaitingForExpenseDescription);

                await bot.SendMessage(
                    message.Chat.Id,
                    "Введите описание расхода:");

                return;
            }

            // ===== ВВОД ОПИСАНИЯ =====
            if (userState == UserState.WaitingForExpenseDescription)
            {
                var description = message.Text;

                if (string.IsNullOrWhiteSpace(description))
                {
                    await bot.SendMessage(
                        message.Chat.Id,
                        "Описание не может быть пустым");
                    return;
                }

                var amount = _storage.GetAmount(userId);

                var user = await _userService.GetByTelegramIdAsync(message.From.Id);

                var dto = new CreateExpenseDto(
                    user.FamilyId,
                    user.Id,
                    Guid.Empty,
                    amount,
                    description,
                    DateTime.UtcNow);

                await _expenseService.AddAsync(dto);

                _state.Clear(userId);
                _storage.Clear(userId);

                await bot.SendMessage(
                    message.Chat.Id,
                    "✅ Расход добавлен!",
                    replyMarkup: KeyboardFactory.MainMenu());
            }
        }
    }
}
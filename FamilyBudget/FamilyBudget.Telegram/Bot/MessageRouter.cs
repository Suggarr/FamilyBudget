using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using FamilyBudget.Core.Enums;
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
        private readonly IncomeHandler _incomeHandler;
        private readonly InviteHandler _inviteHandler;
        private readonly InitialBalanceHandler _initialBalanceHandler;
        private readonly SavingsHandler _savingsHandler;

        public MessageRouter(
            UserStateService state,
            TempExpenseStorage storage,
            IExpenseService expenseService,
            RegistrationHandler registrationHandler,
            IUserService userService,
            IncomeHandler incomeHandler,
            InviteHandler inviteHandler,
            InitialBalanceHandler initialBalanceHandler,
            SavingsHandler savingsHandler)
        {
            _state = state;
            _storage = storage;
            _expenseService = expenseService;
            _registrationHandler = registrationHandler;
            _userService = userService;
            _incomeHandler = incomeHandler;
            _inviteHandler = inviteHandler;
            _initialBalanceHandler = initialBalanceHandler;
            _savingsHandler = savingsHandler;
        }

        public async Task RouteAsync(
            ITelegramBotClient bot,
            Message message)
        {
            var userId = message.From!.Id;
            var userState = _state.Get(userId);

            // Обрабатываем состояния регистрации
            if (userState == UserState.WaitingForFamilyName ||
                userState == UserState.WaitingForUserName ||
                userState == UserState.WaitingForInviteUserName)
            {
                await _registrationHandler.HandleMessageAsync(bot, message);
                return;
            }
            switch (userState)
            {
                // Обрабатываем ввод кода приглашения
                case UserState.WaitingForInviteCode:
                    await _inviteHandler.HandleInviteCodeMessageAsync(bot, message);
                    return;

                case UserState.WaitingForInitialBalance:
                    await _initialBalanceHandler.HandleBalanceAsync(bot, message);
                    return;

                case UserState.WaitingForExpenseAmount:

                    if (!decimal.TryParse(message.Text, out var amount) || amount <= 0)
                    {
                        await bot.SendMessage(
                            message.Chat.Id,
                            "Введите корректную положительную сумму");
                        return;
                    }

                    _storage.SaveAmount(userId, amount);

                    // следующий шаг
                    _state.Set(userId, UserState.WaitingForExpenseDescription);

                    await bot.SendMessage(
                        message.Chat.Id,
                        "Введите описание расхода:");

                    return;

                case UserState.WaitingForExpenseDescription:
                    var description = message.Text;

                    if (string.IsNullOrWhiteSpace(description))
                    {
                        await bot.SendMessage(
                            message.Chat.Id,
                            "Описание не может быть пустым");
                        return;
                    }

                    _storage.SaveDescription(userId, description);

                    _state.Set(userId, UserState.WaitingForExpenseCategory);

                    await bot.SendMessage(
                        message.Chat.Id,
                        "Выберите категорию:",
                        replyMarkup: KeyboardFactory.ExpenseCategories());

                    return;

                case UserState.WaitingForIncomeAmount:
                    await _incomeHandler.HandleAmountAsync(bot, message);
                    break;

                case UserState.WaitingForIncomeDescription:
                    await _incomeHandler.HandleDescriptionAsync(bot, message);
                    break;

                case UserState.WaitingForSavingsContribution:
                    await _savingsHandler.HandleContributionAsync(bot, message);
                    break;

                case UserState.WaitingForSavingsWithdrawal:
                    await _savingsHandler.HandleWithdrawalAsync(bot, message);
                    break;

            }
        }
    }
}

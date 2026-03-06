using FamilyBudget.Application.Services;
using FamilyBudget.Telegram.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Telegram.Handlers
{
    public class ExpenseHandler
    {
        private readonly IExpenseService _expenseService;
        private readonly UserStateService _state;
        private readonly TempExpenseStorage _storage;

        public ExpenseHandler(
            IExpenseService expenseService,
            UserStateService state,
            TempExpenseStorage storage)
        {
            _expenseService = expenseService;
            _state = state;
            _storage = storage;
        }

        public async Task StartAsync(
            ITelegramBotClient bot,
            CallbackQuery query)
        {
            var userId = query.From.Id;

            // сброс предыдущего состояния
            _state.Clear(userId);
            _storage.Clear(userId);

            // запускаем сценарий добавления расхода
            _state.Set(userId, UserState.WaitingForExpenseAmount);

            await bot.SendMessage(
                query.Message!.Chat.Id,
                "💸 Введите сумму расхода:");
        }
    }
}
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using FamilyBudget.Core.Dtos.Expense;
using FamilyBudget.Core.Enums;
using FamilyBudget.Telegram.Keyboards;
using FamilyBudget.Telegram.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Telegram.Handlers
{
    public class ExpenseHandler
    {
        private readonly IExpenseService _expenseService;
        private readonly IUserService _userService;
        private readonly UserStateService _state;
        private readonly TempExpenseStorage _storage;

        public ExpenseHandler(
            IExpenseService expenseService,
            UserStateService state,
            TempExpenseStorage storage,
            IUserService userService)
        {
            _expenseService = expenseService;
            _state = state;
            _storage = storage;
            _userService = userService;
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

        public async Task FinishAsync(
            ITelegramBotClient bot,
            CallbackQuery query,
            ExpenseCategory category)
        {
            var userId = query.From.Id;

            var amount = _storage.GetAmount(userId);
            var description = _storage.GetDescription(userId);

            var user = await _userService.GetByTelegramIdAsync(userId);

            var dto = new CreateExpenseDto(
                user.FamilyId,
                user.Id,
                category,
                amount,
                description,
                DateTime.UtcNow);

            await _expenseService.AddAsync(dto);

            _state.Clear(userId);
            _storage.Clear(userId);

            await bot.SendMessage(
                query.Message!.Chat.Id,
                "✅ Расход добавлен!",
                replyMarkup: KeyboardFactory.MainMenu());
        }
    }
}
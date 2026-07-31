using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using FamilyBudget.Core.Dtos.Expense;
using FamilyBudget.Core.Enums;
using FamilyBudget.Presentation.Telegram.Keyboards;
using FamilyBudget.Presentation.Telegram.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Presentation.Telegram.Handlers
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

        public async Task ShowInputOptionsAsync(
            ITelegramBotClient bot,
            CallbackQuery query)
        {
            _state.Clear(query.From.Id);
            _storage.Clear(query.From.Id);

            await bot.SendMessage(
                query.Message!.Chat.Id,
                "Как добавить расход?",
                replyMarkup: KeyboardFactory.ExpenseInputOptions());
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

            if (user is null || !user.FamilyId.HasValue)
            {
                await bot.SendMessage(query.Message!.Chat.Id, "Пользователь не найден.");
                return;
            }

            var dto = new CreateExpenseDto(
                user.FamilyId.Value,
                user.Id,
                category,
                amount,
                description,
                DateTime.UtcNow);

            try
            {
                await _expenseService.AddAsync(dto);
            }
            catch (InvalidOperationException ex) when (ex.Message == "Insufficient funds.")
            {
                _state.Clear(userId);
                _storage.Clear(userId);
                await bot.SendMessage(
                    query.Message!.Chat.Id,
                    $"Недостаточно средств. На вашем счёте: {user.Balance:F2}",
                    replyMarkup: KeyboardFactory.MainMenu());
                return;
            }

            _state.Clear(userId);
            _storage.Clear(userId);

            await bot.SendMessage(
                query.Message!.Chat.Id,
                "✅ Расход добавлен!",
                replyMarkup: KeyboardFactory.MainMenu());
        }
    }
}

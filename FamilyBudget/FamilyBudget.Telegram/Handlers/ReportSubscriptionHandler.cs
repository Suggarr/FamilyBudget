using System.Globalization;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Telegram.Keyboards;
using FamilyBudget.Telegram.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Telegram.Handlers;

public sealed class ReportSubscriptionHandler
{
    private static readonly string[] TimeFormats = ["H:mm", "HH:mm"];

    private readonly IUserService _userService;
    private readonly IReportSubscriptionService _subscriptionService;
    private readonly UserStateService _state;
    private readonly TempReportSubscriptionStorage _storage;

    public ReportSubscriptionHandler(
        IUserService userService,
        IReportSubscriptionService subscriptionService,
        UserStateService state,
        TempReportSubscriptionStorage storage)
    {
        _userService = userService;
        _subscriptionService = subscriptionService;
        _state = state;
        _storage = storage;
    }

    public async Task ShowAsync(ITelegramBotClient bot, CallbackQuery query)
    {
        ClearInput(query.From.Id);

        var user = await _userService.GetByTelegramIdAsync(query.From.Id);
        if (user is null || !user.FamilyId.HasValue)
        {
            await bot.SendMessage(
                query.Message!.Chat.Id,
                "Вы не зарегистрированы или не состоите в семье.");
            return;
        }

        var subscription = await _subscriptionService.GetByUserIdAsync(user.Id);
        var text = subscription is null
            ? "⏰ Автоматический семейный отчёт не настроен."
            : subscription.IsEnabled
                ? $"""
                   ⏰ Автоматический семейный отчёт включён.

                   День: {DayName(subscription.DayOfWeek)}
                   Время: {subscription.TimeOfDay:HH\:mm} UTC
                   Следующая отправка: {subscription.NextRunAt:dd.MM.yyyy HH:mm} UTC
                   """
                : $"""
                   🔕 Автоматический семейный отчёт отключён.

                   Сохранённое расписание: {DayName(subscription.DayOfWeek)}, {subscription.TimeOfDay:HH\:mm} UTC
                   """;

        await bot.SendMessage(
            query.Message!.Chat.Id,
            text,
            replyMarkup: ReportSubscriptionKeyboard.Manage(subscription?.IsEnabled == true));
    }

    public async Task StartSetupAsync(ITelegramBotClient bot, CallbackQuery query)
    {
        ClearInput(query.From.Id);

        await bot.SendMessage(
            query.Message!.Chat.Id,
            "Выберите день еженедельной отправки отчёта:",
            replyMarkup: ReportSubscriptionKeyboard.SelectDay());
    }

    public async Task SelectDayAsync(
        ITelegramBotClient bot,
        CallbackQuery query,
        DayOfWeek dayOfWeek)
    {
        _storage.SaveDay(query.From.Id, dayOfWeek);
        _state.Set(query.From.Id, UserState.WaitingForReportSubscriptionTime);

        await bot.SendMessage(
            query.Message!.Chat.Id,
            $"Выбран день: {DayName(dayOfWeek)}.\nВведите время в UTC в формате ЧЧ:ММ, например 18:30.",
            replyMarkup: ReportSubscriptionKeyboard.Cancel());
    }

    public async Task HandleTimeAsync(ITelegramBotClient bot, Message message)
    {
        if (!TimeOnly.TryParseExact(
                message.Text,
                TimeFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var timeOfDay))
        {
            await bot.SendMessage(
                message.Chat.Id,
                "Не удалось прочитать время. Введите его в формате ЧЧ:ММ, например 18:30.",
                replyMarkup: ReportSubscriptionKeyboard.Cancel());
            return;
        }

        if (!_storage.TryGetDay(message.From!.Id, out var dayOfWeek))
        {
            ClearInput(message.From.Id);
            await bot.SendMessage(
                message.Chat.Id,
                "Настройка устарела. Начните выбор расписания заново.",
                replyMarkup: ReportPeriodKeyboard.Create());
            return;
        }

        var user = await _userService.GetByTelegramIdAsync(message.From.Id);
        if (user is null || !user.FamilyId.HasValue)
        {
            ClearInput(message.From.Id);
            await bot.SendMessage(
                message.Chat.Id,
                "Вы не зарегистрированы или не состоите в семье.");
            return;
        }

        var subscription = await _subscriptionService.SetScheduleAsync(
            user.Id,
            dayOfWeek,
            timeOfDay);
        ClearInput(message.From.Id);

        await bot.SendMessage(
            message.Chat.Id,
            $"""
             ✅ Автоматический отчёт включён.

             Расписание: {DayName(subscription.DayOfWeek)}, {subscription.TimeOfDay:HH\:mm} UTC
             Следующая отправка: {subscription.NextRunAt:dd.MM.yyyy HH:mm} UTC
             """,
            replyMarkup: ReportSubscriptionKeyboard.Manage(true));
    }

    public async Task DisableAsync(ITelegramBotClient bot, CallbackQuery query)
    {
        var user = await _userService.GetByTelegramIdAsync(query.From.Id);
        if (user is null)
        {
            await bot.SendMessage(query.Message!.Chat.Id, "Пользователь не найден.");
            return;
        }

        var subscription = await _subscriptionService.GetByUserIdAsync(user.Id);
        if (subscription is null)
        {
            await bot.SendMessage(
                query.Message!.Chat.Id,
                "Автоматический отчёт ещё не настроен.",
                replyMarkup: ReportSubscriptionKeyboard.Manage(false));
            return;
        }

        await _subscriptionService.DisableAsync(user.Id);
        ClearInput(query.From.Id);

        await bot.SendMessage(
            query.Message!.Chat.Id,
            "🔕 Автоматический семейный отчёт отключён.",
            replyMarkup: ReportSubscriptionKeyboard.Manage(false));
    }

    private void ClearInput(long telegramId)
    {
        _state.Clear(telegramId);
        _storage.Clear(telegramId);
    }

    private static string DayName(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Monday => "понедельник",
        DayOfWeek.Tuesday => "вторник",
        DayOfWeek.Wednesday => "среда",
        DayOfWeek.Thursday => "четверг",
        DayOfWeek.Friday => "пятница",
        DayOfWeek.Saturday => "суббота",
        DayOfWeek.Sunday => "воскресенье",
        _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek))
    };
}

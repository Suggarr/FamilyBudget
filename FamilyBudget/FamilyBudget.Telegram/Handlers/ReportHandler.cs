using System.Globalization;
using System.Text.RegularExpressions;
using FamilyBudget.Application.Dtos;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Telegram.Keyboards;
using FamilyBudget.Telegram.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Telegram.Handlers;

public sealed class ReportHandler
{
    private const string DateFormat = "dd.MM.yyyy";
    private static readonly Regex PeriodPattern = new(
        @"^\s*(\d{1,2}\.\d{1,2}\.\d{4})\s*[-–—]\s*(\d{1,2}\.\d{1,2}\.\d{4})\s*$",
        RegexOptions.Compiled);

    private readonly IUserService _userService;
    private readonly IFamilyReportService _reportService;
    private readonly UserStateService _state;

    public ReportHandler(
        IUserService userService,
        IFamilyReportService reportService,
        UserStateService state)
    {
        _userService = userService;
        _reportService = reportService;
        _state = state;
    }

    public async Task ShowPeriodsAsync(ITelegramBotClient bot, CallbackQuery query)
    {
        _state.Clear(query.From.Id);

        await bot.SendMessage(
            query.Message!.Chat.Id,
            "📊 Выберите период отчёта:",
            replyMarkup: ReportPeriodKeyboard.Create());
    }

    public Task ShowAllTimeAsync(ITelegramBotClient bot, CallbackQuery query) =>
        SendReportAsync(bot, query.From.Id, query.Message!.Chat.Id, null, null, "за всё время");

    public Task ShowRecentDaysAsync(ITelegramBotClient bot, CallbackQuery query, int days)
    {
        var endDate = DateTime.UtcNow.Date.AddDays(1);
        var startDate = endDate.AddDays(-days);

        return SendReportAsync(
            bot,
            query.From.Id,
            query.Message!.Chat.Id,
            startDate,
            endDate,
            $"за последние {days} дней");
    }

    public async Task StartCustomPeriodAsync(ITelegramBotClient bot, CallbackQuery query)
    {
        _state.Set(query.From.Id, UserState.WaitingForReportPeriod);

        await bot.SendMessage(
            query.Message!.Chat.Id,
            "Введите начальную и конечную даты через дефис.\nНапример: 01.07.2026 - 22.07.2026",
            replyMarkup: ReportPeriodKeyboard.Cancel());
    }

    public async Task HandleCustomPeriodAsync(ITelegramBotClient bot, Message message)
    {
        if (!TryParsePeriod(message.Text, out var startDate, out var endDate))
        {
            await bot.SendMessage(
                message.Chat.Id,
                "Не удалось прочитать период. Используйте формат: 01.07.2026 - 22.07.2026",
                replyMarkup: ReportPeriodKeyboard.Cancel());
            return;
        }

        if (startDate > endDate)
        {
            await bot.SendMessage(
                message.Chat.Id,
                "Начальная дата не может быть позже конечной. Введите период ещё раз.",
                replyMarkup: ReportPeriodKeyboard.Cancel());
            return;
        }

        _state.Clear(message.From!.Id);
        var exclusiveEndDate = endDate.AddDays(1);

        await SendReportAsync(
            bot,
            message.From.Id,
            message.Chat.Id,
            startDate,
            exclusiveEndDate,
            $"с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}");
    }

    public async Task CancelAsync(ITelegramBotClient bot, CallbackQuery query)
    {
        _state.Clear(query.From.Id);
        await bot.SendMessage(
            query.Message!.Chat.Id,
            "Главное меню",
            replyMarkup: KeyboardFactory.MainMenu());
    }

    private async Task SendReportAsync(
        ITelegramBotClient bot,
        long telegramId,
        long chatId,
        DateTime? startDate,
        DateTime? endDate,
        string periodTitle)
    {
        var user = await _userService.GetByTelegramIdAsync(telegramId);
        if (user is null || !user.FamilyId.HasValue)
        {
            await bot.SendMessage(chatId, "Вы не зарегистрированы или не состоите в семье.");
            return;
        }

        var report = await _reportService.GetFamilyReportAsync(
            user.FamilyId.Value,
            startDate,
            endDate);

        await bot.SendMessage(chatId, FormatReport(report, periodTitle));
    }

    private static bool TryParsePeriod(string? text, out DateTime startDate, out DateTime endDate)
    {
        startDate = default;
        endDate = default;

        var match = PeriodPattern.Match(text ?? string.Empty);
        if (!match.Success)
            return false;

        return DateTime.TryParseExact(
                   match.Groups[1].Value,
                   DateFormat,
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                   out startDate) &&
               DateTime.TryParseExact(
                   match.Groups[2].Value,
                   DateFormat,
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                   out endDate);
    }

    private static string FormatReport(FamilyReportDto report, string periodTitle)
    {
        var topCategory = report.TopCategory is null
            ? "Нет расходов"
            : $"{CategoryName(report.TopCategory)} — {report.TopCategoryAmount:0.00} BYN";

        return $"""
               📊 Семейный отчёт {periodTitle}

               💰 Доходы: {report.TotalIncome:0.00} BYN
               💸 Расходы: {report.TotalExpenses:0.00} BYN
               📈 Разница: {report.NetAmount:0.00} BYN
               📉 Доля расходов от доходов: {report.ExpensePercent:0.00} %

               🏆 Больше всего потрачено:
               {topCategory}
               """;
    }

    private static string CategoryName(string category) => category switch
    {
        "Food" => "Еда",
        "Transport" => "Транспорт",
        "Entertainment" => "Развлечения",
        "Health" => "Здоровье",
        "Home" => "Дом",
        _ => "Другое"
    };
}

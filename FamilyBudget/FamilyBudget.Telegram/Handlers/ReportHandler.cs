using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using FamilyBudget.Core.Enums;
using FamilyBudget.Telegram.Keyboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FamilyBudget.Telegram.Handlers
{
    public class ReportHandler
    {
        private readonly IUserService _userService;
        private readonly MonthlyReportService _reportService;

        public ReportHandler(IUserService userService, MonthlyReportService reportService)
        {
            _userService = userService;
            _reportService = reportService;
        }

        public async Task ShowMonths(
            ITelegramBotClient bot,
            Message message)
        {
            var year = DateTime.UtcNow.Year;

            await bot.SendMessage(
                message.Chat.Id,
                "📊 Выберите месяц:",
                replyMarkup: MonthKeyboard.Create(year));
        }

        public async Task HandleMonth(
            ITelegramBotClient bot,
            CallbackQuery query)
        {
            var data = query.Data!.Split(':');

            int year = int.Parse(data[1]);
            int month = int.Parse(data[2]);

            var user = await _userService.GetByTelegramIdAsync(query.From.Id);

            if (user == null || !user.FamilyId.HasValue)
            {
                await bot.SendMessage(query.Message!.Chat.Id, "Вы не зарегистрированы.");
                return;
            }

            var report = await _reportService.GetFamilyMonthlyReportAsync(
                user.FamilyId.Value,
                year,
                month);

            string text =
            $"""
            📊 Отчет за {new DateTime(year, month, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"))}

            💰 Доходы: {report.TotalIncome}
            💸 Расходы: {report.TotalExpenses}

            📈 Баланс: {report.Balance}

            📉 Процент расходов: {report.ExpensePercent:F2} %

            🏆 Больше всего потрачено на:
            {report.TopCategory} — {report.TopCategoryAmount}
            """;

            await bot.SendMessage(
                query.Message!.Chat.Id,
                text);
        }
    }
}

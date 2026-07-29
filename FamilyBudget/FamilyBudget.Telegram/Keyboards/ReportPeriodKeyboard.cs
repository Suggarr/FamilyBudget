using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyBudget.Telegram.Keyboards;

public static class ReportPeriodKeyboard
{
    public static InlineKeyboardMarkup Create() => new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("📚 Всё время", "report_all")
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData("7 дней", "report_7_days"),
            InlineKeyboardButton.WithCallbackData("30 дней", "report_30_days")
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData("✍️ Указать даты", "report_custom")
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData("⏰ Автоматический отчёт", "report_subscription")
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData("⬅️ Назад", "main_menu")
        }
    });

    public static InlineKeyboardMarkup Cancel() => new(
        InlineKeyboardButton.WithCallbackData("✖️ Отмена", "report_cancel"));
}

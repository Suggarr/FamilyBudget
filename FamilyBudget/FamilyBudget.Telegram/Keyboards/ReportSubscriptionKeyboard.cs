using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyBudget.Telegram.Keyboards;

public static class ReportSubscriptionKeyboard
{
    public static InlineKeyboardMarkup Manage(bool isEnabled)
    {
        var rows = new List<InlineKeyboardButton[]>
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    isEnabled ? "✏️ Изменить расписание" : "✅ Настроить и включить",
                    "report_subscription_setup")
            }
        };

        if (isEnabled)
        {
            rows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "🔕 Отключить",
                    "report_subscription_disable")
            });
        }

        rows.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("⬅️ К отчётам", "report")
        });

        return new InlineKeyboardMarkup(rows);
    }

    public static InlineKeyboardMarkup SelectDay() => new(new[]
    {
        DayRow(
            ("Пн", DayOfWeek.Monday),
            ("Вт", DayOfWeek.Tuesday),
            ("Ср", DayOfWeek.Wednesday),
            ("Чт", DayOfWeek.Thursday)),
        DayRow(
            ("Пт", DayOfWeek.Friday),
            ("Сб", DayOfWeek.Saturday),
            ("Вс", DayOfWeek.Sunday)),
        new[]
        {
            InlineKeyboardButton.WithCallbackData("✖️ Отмена", "report_subscription")
        }
    });

    public static InlineKeyboardMarkup Cancel() => new(
        InlineKeyboardButton.WithCallbackData("✖️ Отмена", "report_subscription"));

    private static InlineKeyboardButton[] DayRow(
        params (string Title, DayOfWeek Day)[] days)
    {
        return days
            .Select(day => InlineKeyboardButton.WithCallbackData(
                day.Title,
                $"report_subscription_day:{(int)day.Day}"))
            .ToArray();
    }
}

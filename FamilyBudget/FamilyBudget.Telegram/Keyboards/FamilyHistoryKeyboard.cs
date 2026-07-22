using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyBudget.Telegram.Keyboards;

public static class FamilyHistoryKeyboard
{
    public static InlineKeyboardMarkup Create(int page, int totalPages)
    {
        var navigation = new List<InlineKeyboardButton>();

        if (page > 1)
            navigation.Add(InlineKeyboardButton.WithCallbackData("◀️", $"history_page:{page - 1}"));

        navigation.Add(InlineKeyboardButton.WithCallbackData($"{page}/{totalPages}", "history_noop"));

        if (page < totalPages)
            navigation.Add(InlineKeyboardButton.WithCallbackData("▶️", $"history_page:{page + 1}"));

        return new InlineKeyboardMarkup(new[]
        {
            navigation.ToArray(),
            new[] { InlineKeyboardButton.WithCallbackData("⬅️ Главное меню", "main_menu") }
        });
    }
}

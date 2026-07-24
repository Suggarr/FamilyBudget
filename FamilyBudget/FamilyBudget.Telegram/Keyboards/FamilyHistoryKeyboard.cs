using FamilyBudget.Application.Dtos.History;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyBudget.Telegram.Keyboards;

public static class FamilyHistoryKeyboard
{
    public static InlineKeyboardMarkup Create(FamilyHistoryPageDto history)
    {
        var navigation = new List<InlineKeyboardButton>();

        if (history.Page > 1)
            navigation.Add(InlineKeyboardButton.WithCallbackData("◀️", $"history_page:{history.Page - 1}"));

        navigation.Add(InlineKeyboardButton.WithCallbackData(
            $"{history.Page}/{history.TotalPages}",
            "history_noop"));

        if (history.Page < history.TotalPages)
            navigation.Add(InlineKeyboardButton.WithCallbackData("▶️", $"history_page:{history.Page + 1}"));

        var rows = history.Items
            .Where(operation => operation.ReceiptId.HasValue)
            .Select(operation => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    ReceiptButtonText(operation.Description),
                    $"rv:{operation.ReceiptId!.Value}:1:h:{history.Page}")
            })
            .ToList();

        rows.Add(navigation.ToArray());
        rows.Add([InlineKeyboardButton.WithCallbackData("⬅️ Главное меню", "main_menu")]);
        return new InlineKeyboardMarkup(rows);
    }

    private static string ReceiptButtonText(string description)
    {
        const int maxLength = 32;
        var text = string.IsNullOrWhiteSpace(description) ? "Чек" : description.Trim();
        if (text.Length > maxLength)
            text = $"{text[..(maxLength - 1)]}…";

        return $"🧾 {text}";
    }
}

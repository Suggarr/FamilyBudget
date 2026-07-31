using FamilyBudget.Application.Dtos.Receipt;
using FamilyBudget.Presentation.Telegram.Receipts;
using Telegram.Bot.Types.ReplyMarkups;

namespace FamilyBudget.Presentation.Telegram.Keyboards;

public static class ReceiptHistoryKeyboard
{
    public static InlineKeyboardMarkup CreateList(ReceiptHistoryPageDto history)
    {
        var rows = history.Items
            .Select(receipt => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    ButtonText(receipt),
                    $"rv:{receipt.Id}:1:r:{history.Page}")
            })
            .ToList();
        var navigation = new List<InlineKeyboardButton>();

        if (history.Page > 1)
            navigation.Add(InlineKeyboardButton.WithCallbackData("◀️", $"receipts_page:{history.Page - 1}"));

        navigation.Add(InlineKeyboardButton.WithCallbackData(
            $"{history.Page}/{history.TotalPages}",
            "receipts_noop"));

        if (history.Page < history.TotalPages)
            navigation.Add(InlineKeyboardButton.WithCallbackData("▶️", $"receipts_page:{history.Page + 1}"));

        rows.Add(navigation.ToArray());
        rows.Add([InlineKeyboardButton.WithCallbackData("⬅️ Главное меню", "main_menu")]);
        return new InlineKeyboardMarkup(rows);
    }

    public static InlineKeyboardMarkup CreateDetails(
        Guid receiptId,
        int itemPage,
        int totalItemPages,
        ReceiptNavigationOrigin origin,
        int originPage)
    {
        var navigation = new List<InlineKeyboardButton>();

        if (itemPage > 1)
        {
            navigation.Add(InlineKeyboardButton.WithCallbackData(
                "◀️",
                ViewCallback(receiptId, itemPage - 1, origin, originPage)));
        }

        navigation.Add(InlineKeyboardButton.WithCallbackData(
            $"{itemPage}/{totalItemPages}",
            "receipts_noop"));

        if (itemPage < totalItemPages)
        {
            navigation.Add(InlineKeyboardButton.WithCallbackData(
                "▶️",
                ViewCallback(receiptId, itemPage + 1, origin, originPage)));
        }

        return new InlineKeyboardMarkup(new[]
        {
            navigation.ToArray(),
            new[] { BackButton(origin, originPage) }
        });
    }

    private static InlineKeyboardButton BackButton(ReceiptNavigationOrigin origin, int originPage) =>
        origin == ReceiptNavigationOrigin.FamilyHistory
            ? InlineKeyboardButton.WithCallbackData("⬅️ К истории", $"history_page:{originPage}")
            : InlineKeyboardButton.WithCallbackData("⬅️ К списку чеков", $"receipts_page:{originPage}");

    private static string ViewCallback(
        Guid receiptId,
        int itemPage,
        ReceiptNavigationOrigin origin,
        int originPage)
    {
        var originCode = origin == ReceiptNavigationOrigin.FamilyHistory ? "h" : "r";
        return $"rv:{receiptId}:{itemPage}:{originCode}:{originPage}";
    }

    private static string ButtonText(ReceiptDto receipt)
    {
        const int maxMerchantLength = 24;
        var merchant = string.IsNullOrWhiteSpace(receipt.MerchantName)
            ? "Неизвестный магазин"
            : receipt.MerchantName.Trim();
        if (merchant.Length > maxMerchantLength)
            merchant = $"{merchant[..(maxMerchantLength - 1)]}…";

        return $"{receipt.PurchasedAt:dd.MM} · {merchant} · {receipt.TotalAmount:0.00}";
    }
}

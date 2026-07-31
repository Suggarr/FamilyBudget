using System.Globalization;
using System.Text;
using FamilyBudget.Application.Dtos.Receipt;
using FamilyBudget.Core.Enums;

namespace FamilyBudget.Presentation.Telegram.Receipts;

public static class ReceiptHistoryMessageFormatter
{
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("ru-RU");

    public static string FormatList(ReceiptHistoryPageDto history)
    {
        if (history.Items.Count == 0)
        {
            return """
                   🧾 Чеки семьи

                   Сохранённых чеков пока нет.
                   """;
        }

        var builder = new StringBuilder();
        builder.AppendLine("🧾 Чеки семьи");
        builder.AppendLine($"Страница {history.Page} из {history.TotalPages} · всего чеков: {history.TotalCount}");

        for (var index = 0; index < history.Items.Count; index++)
        {
            var receipt = history.Items[index];
            var merchant = string.IsNullOrWhiteSpace(receipt.MerchantName)
                ? "Неизвестный магазин"
                : receipt.MerchantName.Trim();

            builder.AppendLine();
            builder.AppendLine($"{index + 1}. {StatusIcon(receipt.Status)} {receipt.PurchasedAt:dd.MM.yyyy HH:mm}");
            builder.AppendLine($"{merchant} · {Money(receipt.TotalAmount)} {receipt.Currency}");
        }

        return builder.ToString().TrimEnd();
    }

    public static string FormatDetails(
        ReceiptDto receipt,
        int itemPage,
        int itemPageSize)
    {
        var totalItemPages = Math.Max(1, (int)Math.Ceiling(receipt.Items.Count / (double)itemPageSize));
        var actualItemPage = Math.Clamp(itemPage, 1, totalItemPages);
        var items = receipt.Items
            .Skip((actualItemPage - 1) * itemPageSize)
            .Take(itemPageSize);
        var merchant = string.IsNullOrWhiteSpace(receipt.MerchantName)
            ? "Неизвестный магазин"
            : receipt.MerchantName.Trim();

        var builder = new StringBuilder();
        builder.AppendLine($"🧾 Чек: {merchant}");
        builder.AppendLine($"Дата: {receipt.PurchasedAt:dd.MM.yyyy HH:mm}");
        builder.AppendLine($"Статус: {StatusName(receipt.Status)}");
        builder.AppendLine();
        builder.AppendLine($"Сумма до скидки: {Money(receipt.Subtotal)} {receipt.Currency}");
        builder.AppendLine($"Скидка: {Money(receipt.DiscountAmount)} {receipt.Currency}");
        builder.AppendLine($"Итого к оплате: {Money(receipt.TotalAmount)} {receipt.Currency}");
        builder.AppendLine();
        builder.AppendLine($"Позиции · страница {actualItemPage} из {totalItemPages}:");

        foreach (var item in items)
        {
            var discount = item.DiscountAmount > 0
                ? $" − {Money(item.DiscountAmount)}"
                : string.Empty;

            builder.AppendLine($"• {item.Name}");
            builder.AppendLine($"  {Quantity(item.Quantity)} × {Money(item.UnitPrice)}{discount} = {Money(item.TotalAmount)} {receipt.Currency}");
        }

        return builder.ToString().TrimEnd();
    }

    public static int GetItemPageCount(ReceiptDto receipt, int itemPageSize) =>
        Math.Max(1, (int)Math.Ceiling(receipt.Items.Count / (double)itemPageSize));

    private static string StatusIcon(ReceiptStatus status) => status switch
    {
        ReceiptStatus.Confirmed => "✅",
        ReceiptStatus.Rejected => "❌",
        ReceiptStatus.Failed => "⚠️",
        _ => "⏳"
    };

    private static string StatusName(ReceiptStatus status) => status switch
    {
        ReceiptStatus.Confirmed => "Подтверждён",
        ReceiptStatus.Rejected => "Не учтён",
        ReceiptStatus.Failed => "Ошибка распознавания",
        _ => "Ожидает подтверждения"
    };

    private static string Quantity(decimal value) => value.ToString("0.###", DisplayCulture);

    private static string Money(decimal value) => value.ToString("0.00", DisplayCulture);
}

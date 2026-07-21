using FamilyBudget.Application.Dtos.Receipt;
using System.Globalization;

namespace FamilyBudget.Telegram.Receipts;

public static class ReceiptMessageFormatter
{
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("ru-RU");

    public static string Format(ReceiptDto receipt)
    {
        var merchant = string.IsNullOrWhiteSpace(receipt.MerchantName)
            ? "Неизвестный магазин"
            : receipt.MerchantName;
        var lines = receipt.Items.Take(8)
            .Select(item => FormatItem(item, receipt.Currency));
        var moreItems = receipt.Items.Count > 8 ? "\n• …" : string.Empty;

        return $"🧾 Чек: {merchant}\n" +
               $"Сумма до скидки: {Money(receipt.Subtotal)} {receipt.Currency}\n" +
               $"Скидка чека: {Money(receipt.DiscountAmount)} {receipt.Currency}\n" +
               $"Итого к оплате: {Money(receipt.TotalAmount)} {receipt.Currency}\n\n" +
               $"Позиции:\n{string.Join('\n', lines)}{moreItems}\n\n" +
               "Выберите категорию — это подтвердит и добавит расход.";
    }

    private static string FormatItem(ReceiptItemDto item, string currency)
    {
        var discount = item.DiscountAmount > 0
            ? $" − {Money(item.DiscountAmount)}"
            : string.Empty;

        return $"• {item.Name}\n" +
               $"  {Quantity(item.Quantity)} × {Money(item.UnitPrice)}{discount} = {Money(item.TotalAmount)} {currency}";
    }

    private static string Quantity(decimal value) => value.ToString("0.###", DisplayCulture);

    private static string Money(decimal value) => value.ToString("0.00", DisplayCulture);
}

using FamilyBudget.Application.Dtos.Receipt;
using FamilyBudget.Application.Exceptions;
using FamilyBudget.Core.Models;
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
        var lines = receipt.Items
            .Select(item => FormatItem(item, receipt.Currency));

        return $"🧾 Чек: {merchant}\n" +
               $"Сумма до скидки: {Money(receipt.Subtotal)} {receipt.Currency}\n" +
               $"Скидка чека: {Money(receipt.DiscountAmount)} {receipt.Currency}\n" +
               $"Итого к оплате: {Money(receipt.TotalAmount)} {receipt.Currency}\n\n" +
               $"Позиции:\n{string.Join('\n', lines)}\n\n" +
               "Выберите категорию — это подтвердит и добавит расход.";
    }

    public static string FormatValidationFailure(ReceiptAmountsValidationException exception)
    {
        var receipt = exception.ParsedReceipt;
        var merchant = string.IsNullOrWhiteSpace(receipt.MerchantName)
            ? "Неизвестный магазин"
            : receipt.MerchantName;
        var itemsTotal = receipt.Items.Sum(item => item.TotalAmount);
        var lines = receipt.Items.Select(item => FormatParsedItem(item, receipt.Currency));

        return $"⚠️ Проверка сумм не пройдена\n" +
               $"{exception.Message}\n" +
               "Чек не сохранён.\n\n" +
               $"🧾 Распознанные данные: {merchant}\n" +
               $"Сумма до скидки: {Money(receipt.Subtotal)} {receipt.Currency}\n" +
               $"Скидка чека: {Money(receipt.DiscountAmount)} {receipt.Currency}\n" +
               $"Налог: {Money(receipt.TaxAmount)} {receipt.Currency}\n" +
               $"Итого к оплате: {Money(receipt.TotalAmount)} {receipt.Currency}\n" +
               $"Сумма распознанных позиций: {Money(itemsTotal)} {receipt.Currency}\n\n" +
               $"Позиции:\n{string.Join('\n', lines)}";
    }

    private static string FormatItem(ReceiptItemDto item, string currency)
    {
        var discount = item.DiscountAmount > 0
            ? $" − {Money(item.DiscountAmount)}"
            : string.Empty;

        return $"• {item.Name}\n" +
               $"  {Quantity(item.Quantity)} × {Money(item.UnitPrice)}{discount} = {Money(item.TotalAmount)} {currency}";
    }

    private static string FormatParsedItem(ReceiptParseItem item, string currency)
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

namespace FamilyBudget.Core.Models;

public record ReceiptParseResult(
    string? MerchantName,
    DateTime PurchasedAt,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<ReceiptParseItem> Items,
    string RawResponse);

public record ReceiptParseItem(
    string Name,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TotalAmount);

using FamilyBudget.Core.Enums;

namespace FamilyBudget.Application.Dtos.Receipt;

public record ReceiptDto(
    Guid Id,
    Guid FamilyId,
    Guid UserId,
    string? MerchantName,
    DateTime PurchasedAt,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string Currency,
    string SourceFileId,
    ReceiptStatus Status,
    Guid? ExpenseId,
    IReadOnlyList<ReceiptItemDto> Items);

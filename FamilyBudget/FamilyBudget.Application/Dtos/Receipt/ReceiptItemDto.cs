namespace FamilyBudget.Application.Dtos.Receipt;

public record ReceiptItemDto(
    Guid Id,
    string Name,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TotalAmount);

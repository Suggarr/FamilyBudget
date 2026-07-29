using CSharpFunctionalExtensions;

namespace FamilyBudget.Core.Models;

public class ReceiptItem
{
    public const int MAX_NAME_LENGTH = 300;

    private ReceiptItem(Guid id, string name, decimal quantity, decimal unitPrice, decimal discountAmount, decimal totalAmount)
    {
        Id = id;
        Name = name;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountAmount = discountAmount;
        TotalAmount = totalAmount;
    }

    public Guid Id { get; }
    public string Name { get; }
    public decimal Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal DiscountAmount { get; }
    public decimal TotalAmount { get; }

    public static Result<ReceiptItem> Create(
        Guid id,
        string name,
        decimal quantity,
        decimal unitPrice,
        decimal discountAmount,
        decimal totalAmount)
    {
        if (id == Guid.Empty)
            return Result.Failure<ReceiptItem>("Item id cannot be empty.");
        if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
            return Result.Failure<ReceiptItem>("Item name is invalid.");
        if (quantity <= 0 || unitPrice < 0 || discountAmount < 0 || totalAmount < 0)
            return Result.Failure<ReceiptItem>("Item amounts are invalid.");

        return Result.Success(new ReceiptItem(id, name.Trim(), quantity, unitPrice, discountAmount, totalAmount));
    }
}

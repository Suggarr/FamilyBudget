namespace FamilyBudget.Persistence.Entities;

public class ReceiptItemEntity
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public ReceiptEntity Receipt { get; set; } = null!;
}

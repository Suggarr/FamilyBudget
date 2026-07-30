using FamilyBudget.Core.Enums;

namespace FamilyBudget.Persistence.Entities;

public class ReceiptEntity
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public Guid UserId { get; set; }
    public string? MerchantName { get; set; }
    public DateTime PurchasedAt { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "BYN";
    public string SourceFileId { get; set; } = string.Empty;
    public string RawResponse { get; set; } = string.Empty;
    public ReceiptStatus Status { get; set; }
    public Guid? ExpenseId { get; set; }

    public FamilyEntity Family { get; set; } = null!;
    public UserEntity User { get; set; } = null!;
    public ExpenseEntity? Expense { get; set; }
    public ICollection<ReceiptItemEntity> Items { get; set; } = new List<ReceiptItemEntity>();
}

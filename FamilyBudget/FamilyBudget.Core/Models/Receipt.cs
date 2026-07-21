using CSharpFunctionalExtensions;
using FamilyBudget.Core.Enums;

namespace FamilyBudget.Core.Models;

public class Receipt
{
    public const int MAX_MERCHANT_NAME_LENGTH = 200;
    public const int MAX_CURRENCY_LENGTH = 3;
    public const int MAX_SOURCE_FILE_ID_LENGTH = 512;
    public const int MAX_RAW_RESPONSE_LENGTH = 100_000;

    private readonly List<ReceiptItem> _items;

    private Receipt(
        Guid id,
        Guid familyId,
        Guid userId,
        string? merchantName,
        DateTime purchasedAt,
        decimal subtotal,
        decimal discountAmount,
        decimal taxAmount,
        decimal totalAmount,
        string currency,
        string sourceFileId,
        string rawResponse,
        ReceiptStatus status,
        IEnumerable<ReceiptItem> items)
    {
        Id = id;
        FamilyId = familyId;
        UserId = userId;
        MerchantName = merchantName;
        PurchasedAt = purchasedAt;
        Subtotal = subtotal;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        TotalAmount = totalAmount;
        Currency = currency;
        SourceFileId = sourceFileId;
        RawResponse = rawResponse;
        Status = status;
        _items = items.ToList();
    }

    public Guid Id { get; }
    public Guid FamilyId { get; }
    public Guid UserId { get; }
    public string? MerchantName { get; }
    public DateTime PurchasedAt { get; }
    public decimal Subtotal { get; }
    public decimal DiscountAmount { get; }
    public decimal TaxAmount { get; }
    public decimal TotalAmount { get; }
    public string Currency { get; }
    public string SourceFileId { get; }
    public string RawResponse { get; }
    public ReceiptStatus Status { get; private set; }
    public IReadOnlyList<ReceiptItem> Items => _items;

    public static Result<Receipt> Create(
        Guid id,
        Guid familyId,
        Guid userId,
        string? merchantName,
        DateTime purchasedAt,
        decimal subtotal,
        decimal discountAmount,
        decimal taxAmount,
        decimal totalAmount,
        string currency,
        string sourceFileId,
        string rawResponse,
        IEnumerable<ReceiptItem> items,
        ReceiptStatus status = ReceiptStatus.Parsed)
    {
        if (id == Guid.Empty || familyId == Guid.Empty || userId == Guid.Empty)
            return Result.Failure<Receipt>("Identifiers cannot be empty.");
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length > MAX_CURRENCY_LENGTH)
            return Result.Failure<Receipt>("Currency must be a valid three-letter code.");
        if (string.IsNullOrWhiteSpace(sourceFileId) || sourceFileId.Length > MAX_SOURCE_FILE_ID_LENGTH)
            return Result.Failure<Receipt>("Source file id is required.");
        if (string.IsNullOrWhiteSpace(rawResponse) || rawResponse.Length > MAX_RAW_RESPONSE_LENGTH)
            return Result.Failure<Receipt>("Raw parser response is required and cannot be too long.");
        if (purchasedAt > DateTime.UtcNow)
            return Result.Failure<Receipt>("Purchase date cannot be in the future.");
        if (subtotal < 0 || discountAmount < 0 || taxAmount < 0 || totalAmount <= 0)
            return Result.Failure<Receipt>("Receipt amounts are invalid.");

        var normalizedItems = items.ToList();
        if (normalizedItems.Count == 0)
            return Result.Failure<Receipt>("Receipt must contain at least one item.");

        if (merchantName?.Length > MAX_MERCHANT_NAME_LENGTH)
            return Result.Failure<Receipt>("Merchant name is too long.");

        return Result.Success(new Receipt(
            id,
            familyId,
            userId,
            string.IsNullOrWhiteSpace(merchantName) ? null : merchantName.Trim(),
            purchasedAt,
            subtotal,
            discountAmount,
            taxAmount,
            totalAmount,
            currency.Trim().ToUpperInvariant(),
            sourceFileId.Trim(),
            rawResponse,
            status,
            normalizedItems));
    }

    public Result Confirm()
    {
        if (Status != ReceiptStatus.Parsed)
            return Result.Failure("Only parsed receipts can be confirmed.");

        Status = ReceiptStatus.Confirmed;
        return Result.Success();
    }

    public Result Reject()
    {
        if (Status != ReceiptStatus.Parsed)
            return Result.Failure("Only parsed receipts can be rejected.");

        Status = ReceiptStatus.Rejected;
        return Result.Success();
    }
}

using FamilyBudget.Application.Dtos.Receipt;
using FamilyBudget.Application.Exceptions;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using FamilyBudget.Core.Dtos.Expense;
using FamilyBudget.Core.Enums;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;

namespace FamilyBudget.Application.Services;

public class ReceiptService : IReceiptService
{
    private readonly IReceiptParser _parser;
    private readonly IReceiptRepository _repository;
    private readonly IExpenseService _expenseService;

    public ReceiptService(IReceiptParser parser, IReceiptRepository repository, IExpenseService expenseService)
    {
        _parser = parser;
        _repository = repository;
        _expenseService = expenseService;
    }

    public async Task<ReceiptDto> ParseAndSaveAsync(CreateReceiptDto dto, CancellationToken cancellationToken = default)
    {
        var parsed = await _parser.ParseAsync(dto.ImageBytes, dto.MediaType, cancellationToken);
        var items = new List<ReceiptItem>();

        foreach (var parsedItem in parsed.Items)
        {
            if (Math.Abs(parsedItem.Quantity * parsedItem.UnitPrice - parsedItem.DiscountAmount - parsedItem.TotalAmount) > 0.1m)
            {
                throw new ReceiptAmountsValidationException(
                    $"Не сошлась сумма позиции «{parsedItem.Name}».",
                    parsed);
            }

            var itemResult = ReceiptItem.Create(
                Guid.NewGuid(),
                parsedItem.Name,
                parsedItem.Quantity,
                parsedItem.UnitPrice,
                parsedItem.DiscountAmount,
                parsedItem.TotalAmount);

            if (itemResult.IsFailure)
                throw new InvalidOperationException(itemResult.Error);

            items.Add(itemResult.Value);
        }

        var itemsTotal = items.Sum(item => item.TotalAmount);
        if (Math.Abs(itemsTotal - parsed.TotalAmount) > 0.1m &&
            Math.Abs(itemsTotal - parsed.Subtotal) > 0.1m)
        {
            throw new ReceiptAmountsValidationException(
                "Сумма распознанных позиций не совпала с итогами чека.",
                parsed);
        }

        var receiptResult = Receipt.Create(
            Guid.NewGuid(),
            dto.FamilyId,
            dto.UserId,
            parsed.MerchantName,
            parsed.PurchasedAt,
            parsed.Subtotal,
            parsed.DiscountAmount,
            parsed.TaxAmount,
            parsed.TotalAmount,
            parsed.Currency,
            dto.SourceFileId,
            parsed.RawResponse,
            items);

        if (receiptResult.IsFailure)
            throw new InvalidOperationException(receiptResult.Error);

        await _repository.AddAsync(receiptResult.Value);
        return ToDto(receiptResult.Value);
    }

    public async Task<List<ReceiptDto>> GetByFamilyAsync(Guid familyId)
    {
        var receipts = await _repository.GetByFamilyIdAsync(familyId);
        return receipts.Select(ToDto).ToList();
    }

    public async Task<ReceiptHistoryPageDto> GetFamilyPageAsync(
        Guid familyId,
        int page,
        int pageSize = 5)
    {
        if (familyId == Guid.Empty)
            throw new ArgumentException("Family ID cannot be empty.", nameof(familyId));
        if (page <= 0)
            throw new ArgumentOutOfRangeException(nameof(page), "Page number must be greater than zero.");
        if (pageSize is < 1 or > 20)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 20.");

        var receipts = await _repository.GetByFamilyIdAsync(familyId);
        var totalCount = receipts.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        var actualPage = Math.Min(page, totalPages);
        var items = receipts
            .Skip((actualPage - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDto)
            .ToList();

        return new ReceiptHistoryPageDto(items, actualPage, pageSize, totalCount, totalPages);
    }

    public async Task<ReceiptDto?> GetByIdForFamilyAsync(Guid receiptId, Guid familyId)
    {
        if (receiptId == Guid.Empty || familyId == Guid.Empty)
            return null;

        var receipt = await _repository.GetByIdAsync(receiptId);
        return receipt is not null && receipt.FamilyId == familyId
            ? ToDto(receipt)
            : null;
    }

    public async Task<Guid> ConfirmAndCreateExpenseAsync(Guid receiptId, ExpenseCategory category)
    {
        var receipt = await _repository.GetByIdAsync(receiptId)
            ?? throw new InvalidOperationException("Receipt not found.");

        if (receipt.Status != ReceiptStatus.Parsed)
            throw new InvalidOperationException("Only parsed receipts can be confirmed.");

        var expenseId = await _expenseService.AddAsync(new CreateExpenseDto(
            receipt.FamilyId,
            receipt.UserId,
            category,
            receipt.TotalAmount,
            BuildExpenseDescription(receipt),
            receipt.PurchasedAt));

        var result = receipt.Confirm(expenseId);
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);

        await _repository.UpdateAsync(receipt);
        return expenseId;
    }

    public async Task RejectAsync(Guid receiptId)
    {
        var receipt = await _repository.GetByIdAsync(receiptId)
            ?? throw new InvalidOperationException("Receipt not found.");
        var result = receipt.Reject();
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);

        await _repository.UpdateAsync(receipt);
    }

    private static ReceiptDto ToDto(Receipt receipt) => new(
        receipt.Id,
        receipt.FamilyId,
        receipt.UserId,
        receipt.MerchantName,
        receipt.PurchasedAt,
        receipt.Subtotal,
        receipt.DiscountAmount,
        receipt.TaxAmount,
        receipt.TotalAmount,
        receipt.Currency,
        receipt.SourceFileId,
        receipt.Status,
        receipt.ExpenseId,
        receipt.Items.Select(item => new ReceiptItemDto(
            item.Id,
            item.Name,
            item.Quantity,
            item.UnitPrice,
            item.DiscountAmount,
            item.TotalAmount)).ToList());

    private static string BuildExpenseDescription(Receipt receipt)
    {
        var merchant = string.IsNullOrWhiteSpace(receipt.MerchantName) ? "Чек" : receipt.MerchantName.Trim();
        var itemNames = string.Join(", ", receipt.Items.Take(3).Select(item => item.Name));
        var suffix = receipt.Items.Count > 3 ? ", …" : string.Empty;
        var description = string.IsNullOrWhiteSpace(itemNames)
            ? $"Чек: {merchant}"
            : $"Чек: {merchant} — {itemNames}{suffix}";

        return description.Length <= Expense.MAX_DESCRIPTION_LENGTH
            ? description
            : description[..Expense.MAX_DESCRIPTION_LENGTH];
    }
}

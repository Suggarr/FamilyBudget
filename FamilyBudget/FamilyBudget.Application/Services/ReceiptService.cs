using FamilyBudget.Application.Dtos.Receipt;
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

        var result = receipt.Confirm();
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

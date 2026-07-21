using FamilyBudget.Application.Dtos.Receipt;
using FamilyBudget.Core.Enums;

namespace FamilyBudget.Application.Interfaces;

public interface IReceiptService
{
    Task<ReceiptDto> ParseAndSaveAsync(CreateReceiptDto dto, CancellationToken cancellationToken = default);
    Task<List<ReceiptDto>> GetByFamilyAsync(Guid familyId);
    Task<Guid> ConfirmAndCreateExpenseAsync(Guid receiptId, ExpenseCategory category);
    Task RejectAsync(Guid receiptId);
}

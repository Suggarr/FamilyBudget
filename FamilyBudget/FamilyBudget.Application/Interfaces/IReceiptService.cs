using FamilyBudget.Application.Dtos.Receipt;
using FamilyBudget.Core.Enums;

namespace FamilyBudget.Application.Interfaces;

public interface IReceiptService
{
    Task<ReceiptDto> ParseAndSaveAsync(CreateReceiptDto dto, CancellationToken cancellationToken = default);
    Task<List<ReceiptDto>> GetByFamilyAsync(Guid familyId);
    Task<ReceiptHistoryPageDto> GetFamilyPageAsync(Guid familyId, int page, int pageSize = 5);
    Task<ReceiptDto?> GetByIdForFamilyAsync(Guid receiptId, Guid familyId);
    Task<Guid> ConfirmAndCreateExpenseAsync(Guid receiptId, ExpenseCategory category);
    Task RejectAsync(Guid receiptId);
}

namespace FamilyBudget.Application.Dtos.Receipt;

public record ReceiptHistoryPageDto(
    IReadOnlyList<ReceiptDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

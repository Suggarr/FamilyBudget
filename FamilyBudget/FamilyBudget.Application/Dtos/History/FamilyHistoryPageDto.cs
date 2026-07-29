namespace FamilyBudget.Application.Dtos.History;

public record FamilyHistoryPageDto(
    IReadOnlyList<FamilyOperationDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
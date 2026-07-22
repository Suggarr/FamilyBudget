namespace FamilyBudget.Application.Dtos.History;

public record FamilyOperationDto(
    Guid Id,
    FamilyOperationType Type,
    Guid UserId,
    string UserName,
    decimal Amount,
    string Description,
    DateTime Date,
    Guid? ReceiptId
);

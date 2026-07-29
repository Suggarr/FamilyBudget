namespace FamilyBudget.Application.Dtos.Receipt;

public record CreateReceiptDto(
    Guid FamilyId,
    Guid UserId,
    string SourceFileId,
    byte[] ImageBytes,
    string MediaType);

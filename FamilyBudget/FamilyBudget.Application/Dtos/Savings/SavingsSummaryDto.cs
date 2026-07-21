namespace FamilyBudget.Application.Dtos.Savings;

public record SavingsSummaryDto(decimal TotalAmount, IReadOnlyList<SavingsOperationDto> Operations);

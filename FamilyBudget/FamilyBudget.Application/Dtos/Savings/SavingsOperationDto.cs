namespace FamilyBudget.Application.Dtos.Savings;

public enum SavingsOperationType
{
    Contribution,
    Withdrawal
}

public record SavingsOperationDto(
    Guid Id,
    Guid UserId,
    string UserName,
    decimal Amount,
    DateTime CreatedAt,
    SavingsOperationType Type);

namespace FamilyBudget.Application.Dtos.Savings
{
    public record SavingsStatisticDto(
        decimal PeriodContributions,
        decimal PeriodWithdrawals,
        decimal PeriodNetChange,
        decimal CurrentBalance
    );
}

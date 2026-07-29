namespace FamilyBudget.Application.Dtos
{
    public record MemberStatisticDto(
        string MemberName,
        decimal TotalIncomes,
        decimal TotalExpenses,
        decimal NetAmount
    );
}

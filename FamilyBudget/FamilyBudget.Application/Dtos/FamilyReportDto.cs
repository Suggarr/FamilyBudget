namespace FamilyBudget.Application.Dtos
{
    public record FamilyReportDto(
        DateTime? StartDate,
        DateTime? EndDate,
        decimal TotalIncome,
        decimal TotalExpenses,
        decimal NetAmount,
        decimal ExpensePercent,
        string? TopCategory,
        decimal? TopCategoryAmount
    );
}

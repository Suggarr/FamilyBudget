using FamilyBudget.Core.Enums;

namespace FamilyBudget.Application.Dtos
{
    public record CategoryStatisticDto(
        ExpenseCategory Category,
        decimal Amount,
        decimal Percent
    );
}

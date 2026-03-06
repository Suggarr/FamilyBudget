using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Dtos
{
    public record MonthlyReportDto(
        decimal TotalIncome,
        decimal TotalExpenses,
        decimal Balance,
        decimal ExpensePercent,
        string? TopCategory,
        decimal? TopCategoryAmount);
}

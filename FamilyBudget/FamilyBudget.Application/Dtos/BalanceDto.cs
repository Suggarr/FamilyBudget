using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Dtos
{
    public record BalanceDto(
        decimal TotalIncome,
        decimal TotalExpense,
        decimal Balance);
}

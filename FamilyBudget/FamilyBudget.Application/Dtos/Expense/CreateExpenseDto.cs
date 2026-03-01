using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Core.Dtos.Expense
{
    public record CreateExpenseDto(
        Guid FamilyId,
        Guid UserId,
        Guid CategoryId,
        decimal Amount,
        string Description,
        DateTime Date);
}

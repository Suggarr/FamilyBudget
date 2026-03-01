using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Dtos.Income
{
    public record CreateIncomeDto(
        Guid FamilyId,
        Guid UserId,
        decimal Amount,
        string Description,
        DateTime Date);
}

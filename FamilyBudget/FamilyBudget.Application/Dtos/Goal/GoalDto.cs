using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Dtos.Goal
{
    public record GoalDto(
        Guid Id,
        Guid FamilyId,
        string Title,
        decimal TargetAmount,
        decimal CurrentAmount,
        decimal ProgressPercent,
        bool IsCompleted);
}

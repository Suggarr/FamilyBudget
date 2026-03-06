using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Dtos.Category
{
    public record RenameCategoryDto(
        Guid FamilyId,
        Guid CategoryId,
        string NewName);
}

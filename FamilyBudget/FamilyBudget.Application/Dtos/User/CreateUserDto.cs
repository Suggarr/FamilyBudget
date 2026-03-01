using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Core.Dtos.User
{
    public record CreateUserDto(Guid FamilyId, string Name, long TelegramId);
}

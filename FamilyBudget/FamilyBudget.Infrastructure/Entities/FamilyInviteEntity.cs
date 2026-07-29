using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Entities
{
    public class FamilyInviteEntity
    {
        public Guid Id { get; set; }
        public Guid FamilyId { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public FamilyEntity Family { get; set; } = null!;
    }
}

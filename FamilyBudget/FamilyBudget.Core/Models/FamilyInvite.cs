using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Core.Models
{
    public class FamilyInvite
    {
        public Guid Id { get; private set; }
        public Guid FamilyId { get; private set; }
        public string Code { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime ExpiresAt { get; private set; }

        private FamilyInvite(Guid id, Guid familyId, string code, DateTime createdAt, DateTime expiresAt)
        {
            Id = id;
            FamilyId = familyId;
            Code = code;
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
        }

        public static Result<FamilyInvite> Create(Guid id, Guid familyId, string code, DateTime createdAt, DateTime expiresAt)
        {
            if(createdAt >= expiresAt)
                return Result.Failure<FamilyInvite>("CreatedAt must be earlier than ExpiresAt.");

            var invite = new FamilyInvite(id, familyId, code, createdAt, expiresAt);
            return Result.Success(invite);
        }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public void GenerateCode()
        {
            Code = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8);
        }

        public void SetLifeTime(TimeSpan lifetime)
        {
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = CreatedAt.Add(lifetime);
        }

    }
}

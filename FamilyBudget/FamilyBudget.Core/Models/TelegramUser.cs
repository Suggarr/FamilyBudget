using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Core.Models
{
    public class TelegramUser
    {
        public Guid Id { get; private set; }
        public long TelegramId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid FamilyId { get; private set; }

        private TelegramUser() { }

        private TelegramUser(Guid id, long telegramId, Guid userId, Guid familyId)
        {
            Id = id;
            TelegramId = telegramId;
            UserId = userId;
            FamilyId = familyId;
        }

        public static TelegramUser Create(
            Guid id,
            long telegramId,
            Guid userId,
            Guid familyId)
        {
            return new TelegramUser(id, telegramId, userId, familyId);
        }
    }
}

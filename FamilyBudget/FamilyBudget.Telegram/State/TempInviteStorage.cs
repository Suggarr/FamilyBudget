using System.Collections.Generic;

namespace FamilyBudget.Telegram.State
{
    public class TempInviteStorage
    {
        private readonly Dictionary<long, string> _inviteCodes = new();
        private readonly Dictionary<long, Guid> _inviteFamilyIds = new();
        private readonly Dictionary<long, string> _suggestedNames = new();

        public void SaveInviteCode(long userId, string code)
        {
            _inviteCodes[userId] = code;
        }

        public string? GetInviteCode(long userId)
        {
            return _inviteCodes.TryGetValue(userId, out var code) ? code : null;
        }

        public void SaveFamilyId(long userId, Guid familyId)
        {
            _inviteFamilyIds[userId] = familyId;
        }

        public Guid? GetFamilyId(long userId)
        {
            return _inviteFamilyIds.TryGetValue(userId, out var id) ? id : null;
        }

        public void SaveSuggestedName(long userId, string name)
        {
            _suggestedNames[userId] = name;
        }

        public string? GetSuggestedName(long userId)
        {
            return _suggestedNames.TryGetValue(userId, out var name) ? name : null;
        }

        public void Clear(long userId)
        {
            _inviteCodes.Remove(userId);
            _inviteFamilyIds.Remove(userId);
            _suggestedNames.Remove(userId);
        }
    }
}

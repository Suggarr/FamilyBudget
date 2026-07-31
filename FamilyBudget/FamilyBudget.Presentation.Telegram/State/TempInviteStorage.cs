using System.Collections.Concurrent;

namespace FamilyBudget.Presentation.Telegram.State
{
    public class TempInviteStorage
    {
        private readonly ConcurrentDictionary<long, string> _inviteCodes = new();
        private readonly ConcurrentDictionary<long, string> _suggestedNames = new();

        public void SaveInviteCode(long userId, string code)
        {
            _inviteCodes[userId] = code;
        }

        public string? GetInviteCode(long userId)
        {
            return _inviteCodes.TryGetValue(userId, out var code) ? code : null;
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
            _inviteCodes.TryRemove(userId, out _);
            _suggestedNames.TryRemove(userId, out _);
        }
    }
}

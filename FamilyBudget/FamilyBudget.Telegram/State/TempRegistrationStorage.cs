using System.Collections.Concurrent;

namespace FamilyBudget.Telegram.State;

public class TempRegistrationStorage
{
    private readonly ConcurrentDictionary<long, string> _familyNames = new();

    public void SaveFamilyName(long userId, string familyName) => _familyNames[userId] = familyName;

    public string? GetFamilyName(long userId) =>
        _familyNames.TryGetValue(userId, out var familyName) ? familyName : null;

    public void Clear(long userId) => _familyNames.TryRemove(userId, out _);
}

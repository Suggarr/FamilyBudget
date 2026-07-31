using System.Collections.Concurrent;

namespace FamilyBudget.Presentation.Telegram.State;

public sealed class TempReportSubscriptionStorage
{
    private readonly ConcurrentDictionary<long, DayOfWeek> _days = new();

    public void SaveDay(long telegramId, DayOfWeek dayOfWeek) =>
        _days[telegramId] = dayOfWeek;

    public bool TryGetDay(long telegramId, out DayOfWeek dayOfWeek) =>
        _days.TryGetValue(telegramId, out dayOfWeek);

    public void Clear(long telegramId) =>
        _days.TryRemove(telegramId, out _);
}

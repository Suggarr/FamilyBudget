using FamilyBudget.Application.Dtos.ReportSubscription;

namespace FamilyBudget.Application.Interfaces;

public interface IReportSubscriptionService
{
    Task<ReportSubscriptionDto?> GetByUserIdAsync(Guid userId);
    Task<IReadOnlyList<ReportSubscriptionDto>> GetDueAsync(DateTime utcNow);
    Task<ReportSubscriptionDto> SetScheduleAsync(Guid userId, DayOfWeek dayOfWeek, TimeOnly timeOfDay);
    Task ScheduleNextRunAsync(Guid userId, DateTime utcNow);
    Task DisableAsync(Guid userId);
}

namespace FamilyBudget.Application.Dtos.ReportSubscription;

public record ReportSubscriptionDto(
    Guid Id,
    Guid UserId,
    bool IsEnabled,
    DayOfWeek DayOfWeek,
    TimeOnly TimeOfDay,
    DateTime NextRunAt);

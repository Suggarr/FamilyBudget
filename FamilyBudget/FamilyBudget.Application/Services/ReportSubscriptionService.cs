using FamilyBudget.Application.Dtos.ReportSubscription;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;

namespace FamilyBudget.Application.Services;

public class ReportSubscriptionService : IReportSubscriptionService
{
    private readonly IReportSubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;

    public ReportSubscriptionService(
        IReportSubscriptionRepository subscriptionRepository,
        IUserRepository userRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _userRepository = userRepository;
    }

    public async Task<ReportSubscriptionDto?> GetByUserIdAsync(Guid userId)
    {
        ValidateUserId(userId);

        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId);

        return subscription is null ? null : Map(subscription);
    }

    public async Task<IReadOnlyList<ReportSubscriptionDto>> GetDueAsync(DateTime utcNow)
    {
        ValidateUtc(utcNow);

        var subscriptions = await _subscriptionRepository.GetDueAsync(utcNow);

        return subscriptions.Select(Map).ToList();
    }

    public async Task<ReportSubscriptionDto> SetScheduleAsync(
        Guid userId,
        DayOfWeek dayOfWeek,
        TimeOnly timeOfDay)
    {
        ValidateUserId(userId);

        if (!Enum.IsDefined(dayOfWeek))
            throw new ArgumentOutOfRangeException(nameof(dayOfWeek), "Invalid day of week.");

        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        if (!user.FamilyId.HasValue)
            throw new InvalidOperationException("User must belong to a family.");

        var nextRunAt = CalculateNextRunAt(dayOfWeek, timeOfDay, DateTime.UtcNow);
        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId);

        if (subscription is null)
        {
            var createResult = ReportSubscription.Create(
                Guid.NewGuid(),
                userId,
                true,
                dayOfWeek,
                timeOfDay,
                nextRunAt);

            if (createResult.IsFailure)
                throw new InvalidOperationException(createResult.Error);

            subscription = createResult.Value;
            await _subscriptionRepository.AddAsync(subscription);
        }
        else
        {
            var changeResult = subscription.ChangeSchedule(dayOfWeek, timeOfDay, nextRunAt);
            if (changeResult.IsFailure)
                throw new InvalidOperationException(changeResult.Error);

            if (!subscription.IsEnabled)
            {
                var enableResult = subscription.Enable(nextRunAt);
                if (enableResult.IsFailure)
                    throw new InvalidOperationException(enableResult.Error);
            }

            await _subscriptionRepository.UpdateAsync(subscription);
        }

        return Map(subscription);
    }

    public async Task ScheduleNextRunAsync(Guid userId, DateTime utcNow)
    {
        ValidateUserId(userId);
        ValidateUtc(utcNow);

        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("Report subscription not found.");

        var nextRunAt = subscription.NextRunAt;
        do
        {
            nextRunAt = nextRunAt.AddDays(7);
        }
        while (nextRunAt <= utcNow);

        var scheduleResult = subscription.ScheduleNextRun(nextRunAt);
        if (scheduleResult.IsFailure)
            throw new InvalidOperationException(scheduleResult.Error);

        await _subscriptionRepository.UpdateAsync(subscription);
    }

    public async Task DisableAsync(Guid userId)
    {
        ValidateUserId(userId);

        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("Report subscription not found.");

        if (!subscription.IsEnabled)
            return;

        subscription.Disable();
        await _subscriptionRepository.UpdateAsync(subscription);
    }

    private static DateTime CalculateNextRunAt(
        DayOfWeek dayOfWeek,
        TimeOnly timeOfDay,
        DateTime utcNow)
    {
        var daysUntilRun = ((int)dayOfWeek - (int)utcNow.DayOfWeek + 7) % 7;
        var runDate = DateOnly.FromDateTime(utcNow).AddDays(daysUntilRun);
        var nextRunAt = DateTime.SpecifyKind(runDate.ToDateTime(timeOfDay), DateTimeKind.Utc);

        return nextRunAt > utcNow ? nextRunAt : nextRunAt.AddDays(7);
    }

    private static ReportSubscriptionDto Map(ReportSubscription subscription)
    {
        return new ReportSubscriptionDto(
            subscription.Id,
            subscription.UserId,
            subscription.IsEnabled,
            subscription.DayOfWeek,
            subscription.TimeOfDay,
            subscription.NextRunAt);
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
    }

    private static void ValidateUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("DateTime must be in UTC.", nameof(value));
    }
}

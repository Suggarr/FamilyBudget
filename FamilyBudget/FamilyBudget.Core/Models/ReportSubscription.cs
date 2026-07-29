using CSharpFunctionalExtensions;

namespace FamilyBudget.Core.Models
{
    public class ReportSubscription
    {
        private ReportSubscription(Guid id, Guid userId, bool isEnabled, DayOfWeek dayOfWeek, TimeOnly timeOfDay, DateTime nextRunAt)
        {
            Id = id;
            UserId = userId;
            IsEnabled = isEnabled;
            DayOfWeek = dayOfWeek;
            TimeOfDay = timeOfDay;
            NextRunAt = nextRunAt;
        }

        public Guid Id { get; }
        public Guid UserId { get; }
        public bool IsEnabled { get; private set; }
        public DayOfWeek DayOfWeek { get; private set; }
        public TimeOnly TimeOfDay { get; private set; }
        public DateTime NextRunAt { get; private set; }

        public static Result<ReportSubscription> Create(Guid id, Guid userId, bool isEnabled, DayOfWeek dayOfWeek, TimeOnly timeOfDay, DateTime nextRunAt)
        {
            if (id == Guid.Empty)
            {
                return Result.Failure<ReportSubscription>("Id cannot be empty.");
            }

            if (userId == Guid.Empty)
            {
                return Result.Failure<ReportSubscription>("UserId cannot be empty.");
            }

            var validation = ValidateSchedule(dayOfWeek, timeOfDay, nextRunAt);
            if (validation.IsFailure)
            {
                return Result.Failure<ReportSubscription>(validation.Error);
            }

            var subscription = new ReportSubscription(id, userId, isEnabled, dayOfWeek, timeOfDay, nextRunAt);
            return Result.Success(subscription);
        }

        public Result Enable(DateTime nextRunAt)
        {
            var validation = ValidateSchedule(DayOfWeek, TimeOfDay, nextRunAt);

            if (validation.IsFailure)
                return validation;

            if (nextRunAt <= DateTime.UtcNow)
            {
                return Result.Failure("NextRunAt must be in the future.");
            }

            IsEnabled = true;
            NextRunAt = nextRunAt;

            return Result.Success();
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        public Result ChangeSchedule(DayOfWeek dayOfWeek, TimeOnly timeOfDay, DateTime nextRunAt)
        {
            var validation = ValidateSchedule(dayOfWeek, timeOfDay, nextRunAt);
            if (validation.IsFailure)
            {
                return Result.Failure(validation.Error);
            }

            if (nextRunAt <= DateTime.UtcNow)
                return Result.Failure("NextRunAt must be in the future.");

            DayOfWeek = dayOfWeek;
            TimeOfDay = timeOfDay;
            NextRunAt = nextRunAt;

            return Result.Success();
        }

        public Result ScheduleNextRun(DateTime nextRunAt)
        {
            if (!IsEnabled)
            {
                return Result.Failure("Cannot schedule next run for a disabled subscription.");
            }

            var validation = ValidateSchedule(DayOfWeek, TimeOfDay, nextRunAt);

            if (validation.IsFailure)
                return validation;

            if (nextRunAt <= DateTime.UtcNow)
            {
                return Result.Failure("NextRunAt must be in the future.");
            }

            NextRunAt = nextRunAt;

            return Result.Success();
        }

        private static Result ValidateSchedule(DayOfWeek dayOfWeek, TimeOnly timeOfDay, DateTime nextRunAt)
        {
            if (!Enum.IsDefined(typeof(DayOfWeek), dayOfWeek))
            {
                return Result.Failure("Invalid DayOfWeek value.");
            }

            if (nextRunAt.Kind != DateTimeKind.Utc)
            {
                return Result.Failure("NextRunAt must be in UTC.");
            }

            if (nextRunAt.DayOfWeek != dayOfWeek)
            {
                return Result.Failure("NextRunAt must match the subscription's schedule.");
            }

            if (TimeOnly.FromDateTime(nextRunAt) != timeOfDay)
            {
                return Result.Failure("NextRunAt must match the subscription's schedule.");
            }

            return Result.Success();
        }
    }
}

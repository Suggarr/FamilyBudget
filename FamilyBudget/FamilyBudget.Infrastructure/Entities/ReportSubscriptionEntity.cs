namespace FamilyBudget.Infrastructure.Entities
{
    public class ReportSubscriptionEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool IsEnabled { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly TimeOfDay { get; set; }
        public DateTime NextRunAt { get; set; }

        public UserEntity User { get; set; } = null!;
    }
}

using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyBudget.Infrastructure.Configurations
{
    public class ReportSubscriptionConfiguration : IEntityTypeConfiguration<ReportSubscriptionEntity>
    {
        public void Configure(EntityTypeBuilder<ReportSubscriptionEntity> builder)
        {
            builder.HasKey(rs => rs.Id);

            builder.Property(rs => rs.IsEnabled)
                .IsRequired();

            builder.Property(rs => rs.DayOfWeek)
                .HasConversion<int>()
                .IsRequired();
            
            builder.Property(rs => rs.TimeOfDay)
                .HasColumnType("time without time zone")
                .IsRequired();

            builder.Property(rs => rs.NextRunAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.HasOne(rs => rs.User)
                .WithOne(u => u.ReportSubscription)
                .HasForeignKey<ReportSubscriptionEntity>(rs => rs.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(rs => new { rs.IsEnabled, rs.NextRunAt});
        }
    }
}

using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyBudget.Infrastructure.Configurations;

public class SavingsContributionConfiguration : IEntityTypeConfiguration<SavingsContributionEntity>
{
    public void Configure(EntityTypeBuilder<SavingsContributionEntity> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Amount).HasPrecision(18, 2);
        builder.HasIndex(c => new { c.FamilyId, c.CreatedAt });

        builder.HasOne(c => c.Family)
            .WithMany(f => f.SavingsContributions)
            .HasForeignKey(c => c.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
            .WithMany(u => u.SavingsContributions)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

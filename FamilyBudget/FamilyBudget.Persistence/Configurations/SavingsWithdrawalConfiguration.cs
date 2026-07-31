using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyBudget.Persistence.Configurations;

public class SavingsWithdrawalConfiguration : IEntityTypeConfiguration<SavingsWithdrawalEntity>
{
    public void Configure(EntityTypeBuilder<SavingsWithdrawalEntity> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Amount).HasPrecision(18, 2);
        builder.HasIndex(w => new { w.FamilyId, w.CreatedAt });

        builder.HasOne(w => w.Family)
            .WithMany(f => f.SavingsWithdrawals)
            .HasForeignKey(w => w.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.User)
            .WithMany(u => u.SavingsWithdrawals)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

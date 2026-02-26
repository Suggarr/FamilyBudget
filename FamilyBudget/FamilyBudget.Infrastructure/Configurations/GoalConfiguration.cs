using FamilyBudgetBot.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Configurations
{
    public class GoalConfiguration : IEntityTypeConfiguration<Goal>
    {
        public void Configure(EntityTypeBuilder<Goal> builder)
        {
            builder.HasKey(g => g.Id);

            builder.Property(g => g.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(g => g.TargetAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(g => g.CurrentAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.HasOne(g => g.Family)
                .WithMany(g => g.Goals)
                .HasForeignKey(g => g.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

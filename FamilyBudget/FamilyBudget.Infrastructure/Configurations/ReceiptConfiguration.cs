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
    public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
    {
        public void Configure(EntityTypeBuilder<Receipt> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.FilePath)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(r => r.IsProcessed)
                .IsRequired();

            builder.Property(r => r.TotalAmount)
                .HasPrecision(18, 2);

            builder.HasOne(r => r.User)
                .WithMany(u => u.Receipts)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Family)
                .WithMany()
                .HasForeignKey(r => r.FamilyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

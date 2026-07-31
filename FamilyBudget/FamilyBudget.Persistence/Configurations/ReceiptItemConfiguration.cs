using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyBudget.Persistence.Configurations;

public class ReceiptItemConfiguration : IEntityTypeConfiguration<ReceiptItemEntity>
{
    public void Configure(EntityTypeBuilder<ReceiptItemEntity> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Name).HasMaxLength(ReceiptItem.MAX_NAME_LENGTH).IsRequired();
        builder.Property(i => i.Quantity).HasPrecision(18, 3);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.DiscountAmount).HasPrecision(18, 2);
        builder.Property(i => i.TotalAmount).HasPrecision(18, 2);
    }
}

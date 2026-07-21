using FamilyBudget.Core.Models;
using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyBudget.Infrastructure.Configurations;

public class ReceiptConfiguration : IEntityTypeConfiguration<ReceiptEntity>
{
    public void Configure(EntityTypeBuilder<ReceiptEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.MerchantName).HasMaxLength(Receipt.MAX_MERCHANT_NAME_LENGTH);
        builder.Property(r => r.Currency).HasMaxLength(Receipt.MAX_CURRENCY_LENGTH).IsRequired();
        builder.Property(r => r.SourceFileId).HasMaxLength(Receipt.MAX_SOURCE_FILE_ID_LENGTH).IsRequired();
        builder.Property(r => r.RawResponse).HasMaxLength(Receipt.MAX_RAW_RESPONSE_LENGTH).IsRequired();
        builder.Property(r => r.Subtotal).HasPrecision(18, 2);
        builder.Property(r => r.DiscountAmount).HasPrecision(18, 2);
        builder.Property(r => r.TaxAmount).HasPrecision(18, 2);
        builder.Property(r => r.TotalAmount).HasPrecision(18, 2);
        builder.HasIndex(r => new { r.FamilyId, r.PurchasedAt });

        builder.HasOne(r => r.Family).WithMany(f => f.Receipts).HasForeignKey(r => r.FamilyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.User).WithMany(u => u.Receipts).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(r => r.Items).WithOne(i => i.Receipt).HasForeignKey(i => i.ReceiptId).OnDelete(DeleteBehavior.Cascade);
    }
}

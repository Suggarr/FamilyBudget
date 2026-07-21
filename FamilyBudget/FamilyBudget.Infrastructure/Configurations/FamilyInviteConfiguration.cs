using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyBudget.Infrastructure.Configurations
{
    public class FamilyInviteConfiguration : IEntityTypeConfiguration<FamilyInviteEntity>
    {
        public void Configure(EntityTypeBuilder<FamilyInviteEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(x => x.Code)
                .IsUnique();

            builder.Property(x => x.CreatedAt).IsRequired();

            builder.Property(x => x.ExpiresAt).IsRequired();
        }
    }
}
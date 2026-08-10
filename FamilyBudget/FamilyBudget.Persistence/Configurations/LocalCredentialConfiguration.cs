using FamilyBudget.Core.Models.Auth;
using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyBudget.Persistence.Configurations;

public sealed class LocalCredentialConfiguration : IEntityTypeConfiguration<LocalCredentialEntity>
{
    public void Configure(EntityTypeBuilder<LocalCredentialEntity> builder)
    {
        builder.ToTable("LocalCredentials");
        builder.HasKey(credential => credential.AccountId);

        builder.Property(credential => credential.Email)
            .IsRequired()
            .HasMaxLength(LocalCredential.MaxEmailLength);

        builder.Property(credential => credential.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(LocalCredential.MaxEmailLength);

        builder.HasIndex(credential => credential.NormalizedEmail)
            .IsUnique();

        builder.Property(credential => credential.PasswordHash)
            .IsRequired()
            .HasMaxLength(LocalCredential.MaxPasswordHashLength);

        builder.Property(credential => credential.CreatedAtUtc)
            .IsRequired();
    }
}

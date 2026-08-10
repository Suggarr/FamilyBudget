using FamilyBudget.Core.Models.Auth;
using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyBudget.Persistence.Configurations;

public sealed class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLoginEntity>
{
    public void Configure(EntityTypeBuilder<ExternalLoginEntity> builder)
    {
        builder.ToTable("ExternalLogins");
        builder.HasKey(login => login.Id);

        builder.Property(login => login.Provider)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(login => login.ProviderSubject)
            .IsRequired()
            .HasMaxLength(ExternalLogin.MaxProviderSubjectLength);

        builder.Property(login => login.ProviderUsername)
            .HasMaxLength(ExternalLogin.MaxProviderUsernameLength);

        builder.Property(login => login.CreatedAtUtc)
            .IsRequired();

        builder.Property(login => login.LastSeenAtUtc)
            .IsRequired();

        builder.HasIndex(login => new { login.Provider, login.ProviderSubject })
            .IsUnique();

        builder.HasIndex(login => new { login.AccountId, login.Provider })
            .IsUnique();
    }
}

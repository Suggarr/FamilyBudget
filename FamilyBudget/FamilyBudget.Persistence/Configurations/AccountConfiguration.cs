using FamilyBudget.Core.Models.Auth;
using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyBudget.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<AccountEntity>
{
    public void Configure(EntityTypeBuilder<AccountEntity> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(account => account.Id);

        builder.Property(account => account.DisplayName)
            .IsRequired()
            .HasMaxLength(Account.MaxDisplayNameLength);

        builder.Property(account => account.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(account => account.LocalCredential)
            .WithOne(credential => credential.Account)
            .HasForeignKey<LocalCredentialEntity>(credential => credential.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(account => account.Sessions)
            .WithOne(session => session.Account)
            .HasForeignKey(session => session.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

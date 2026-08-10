using FamilyBudget.Core.Models.Auth;
using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyBudget.Persistence.Configurations;

public sealed class AuthSessionConfiguration : IEntityTypeConfiguration<AuthSessionEntity>
{
    public void Configure(EntityTypeBuilder<AuthSessionEntity> builder)
    {
        builder.ToTable("AuthSessions");
        builder.HasKey(session => session.Id);

        builder.Property(session => session.DeviceId)
            .IsRequired()
            .HasMaxLength(AuthSession.MaxDeviceIdLength);

        builder.Property(session => session.DeviceName)
            .IsRequired()
            .HasMaxLength(AuthSession.MaxDeviceNameLength);

        builder.Property(session => session.CreatedAtUtc)
            .IsRequired();

        builder.Property(session => session.LastSeenAtUtc)
            .IsRequired();

        builder.Property(session => session.ExpiresAtUtc)
            .IsRequired();

        builder.Property(session => session.RevocationReason)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(session => new { session.AccountId, session.DeviceId });
        builder.HasIndex(session => session.ExpiresAtUtc);

        builder.HasMany(session => session.RefreshTokens)
            .WithOne(token => token.Session)
            .HasForeignKey(token => token.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<FamilyEntity> Families { get; set; }
        public DbSet<ExpenseEntity> Expenses { get; set; }
        public DbSet<GoalEntity> Goals { get; set; }
        public DbSet<IncomeEntity> Incomes { get; set; }
        public DbSet<FamilyInviteEntity> FamilyInvites { get; set; }
        public DbSet<SavingsContributionEntity> SavingsContributions { get; set; }
        public DbSet<SavingsWithdrawalEntity> SavingsWithdrawals { get; set; }
        public DbSet<ReceiptEntity> Receipts { get; set; }
        public DbSet<ReceiptItemEntity> ReceiptItems { get; set; }
        public DbSet<ReportSubscriptionEntity> ReportSubscriptions { get; set; }
        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<LocalCredentialEntity> LocalCredentials { get; set; }
        public DbSet<AuthSessionEntity> AuthSessions { get; set; }
        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
        public DbSet<ExternalLoginEntity> ExternalLogins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}

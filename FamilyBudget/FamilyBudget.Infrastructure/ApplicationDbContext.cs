using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Infrastructure
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}

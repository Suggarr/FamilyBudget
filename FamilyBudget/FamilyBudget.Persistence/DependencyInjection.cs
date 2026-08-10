using FamilyBudget.Core.Interfaces;
using FamilyBudget.Persistence.Mapper;
using FamilyBudget.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyBudget.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddAutoMapper(typeof(UserMappingProfile).Assembly);

        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IFamilyInviteRepository, FamilyInviteRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IIncomeRepository, IncomeRepository>();
        services.AddScoped<IGoalRepository, GoalRepository>();
        services.AddScoped<ISavingsContributionRepository, SavingsContributionRepository>();
        services.AddScoped<ISavingsWithdrawalRepository, SavingsWithdrawalRepository>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<IReportSubscriptionRepository, ReportSubscriptionRepository>();
        services.AddScoped<IFinanceWriter, FinanceWriter>();
        services.AddScoped<IFamilyMembershipWriter, FamilyMembershipWriter>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAuthSessionRepository, AuthSessionRepository>();
        services.AddScoped<IExternalLoginRepository, ExternalLoginRepository>();

        return services;
    }
}

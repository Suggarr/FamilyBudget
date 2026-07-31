using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyBudget.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Mappings));

        services.AddScoped<IFamilyService, FamilyService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IIncomeService, IncomeService>();
        services.AddScoped<IFamilyInviteService, FamilyInviteService>();
        services.AddScoped<IGoalService, GoalService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<ISavingsService, SavingsService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IFamilyReportService, FamilyReportService>();
        services.AddScoped<IFamilyHistoryService, FamilyHistoryService>();
        services.AddScoped<IReportSubscriptionService, ReportSubscriptionService>();

        return services;
    }
}

using FamilyBudget.Presentation.Telegram.Bot;
using FamilyBudget.Presentation.Telegram.Handlers;
using FamilyBudget.Presentation.Telegram.Receipts;
using FamilyBudget.Presentation.Telegram.Reports;
using FamilyBudget.Presentation.Telegram.State;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace FamilyBudget.Presentation.Telegram;

public static class DependencyInjection
{
    public static IServiceCollection AddTelegramBot(
        this IServiceCollection services,
        string telegramToken)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        services.AddSingleton<ITelegramBotClient>(
            new TelegramBotClient(telegramToken, httpClient));

        services.AddScoped<BotHandler>();
        services.AddScoped<CommandRouter>();
        services.AddScoped<CallbackRouter>();
        services.AddScoped<MessageRouter>();

        services.AddScoped<StartHandler>();
        services.AddScoped<RegistrationHandler>();
        services.AddScoped<ExpenseHandler>();
        services.AddScoped<IncomeHandler>();
        services.AddScoped<InitialBalanceHandler>();
        services.AddScoped<ReportHandler>();
        services.AddScoped<InviteHandler>();
        services.AddScoped<FamilyHandler>();
        services.AddScoped<SavingsHandler>();
        services.AddScoped<ReceiptHandler>();
        services.AddScoped<FamilyHistoryHandler>();
        services.AddScoped<ReceiptHistoryHandler>();
        services.AddScoped<ReportSubscriptionHandler>();

        services.AddSingleton<ReceiptProcessingQueue>();
        services.AddHostedService<ReceiptProcessingWorker>();
        services.AddHostedService<ReportSubscriptionWorker>();
        services.AddHostedService<TelegramPollingService>();

        services.AddSingleton<UserStateService>();
        services.AddSingleton<TempExpenseStorage>();
        services.AddSingleton<TempIncomeStorage>();
        services.AddSingleton<TempInviteStorage>();
        services.AddSingleton<TempRegistrationStorage>();
        services.AddSingleton<TempReportSubscriptionStorage>();

        return services;
    }
}

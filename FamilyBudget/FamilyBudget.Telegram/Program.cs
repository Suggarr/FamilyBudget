using FamilyBudget.Application;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Infrastructure;
using FamilyBudget.Infrastructure.AI;
using FamilyBudget.Infrastructure.Configuration;
using FamilyBudget.Infrastructure.Mapper;
using FamilyBudget.Infrastructure.Repositories;
using FamilyBudget.Telegram.Bot;
using FamilyBudget.Telegram.Handlers;
using FamilyBudget.Telegram.Receipts;
using FamilyBudget.Telegram.Reports;
using FamilyBudget.Telegram.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.AI;
using OllamaSharp;
using Telegram.Bot;
using Telegram.Bot.Polling;

EnvironmentFileLoader.Load();
var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("FamilyBudgetDbContext");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings__FamilyBudgetDbContext is not configured.");

// ================= DATABASE =================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        connectionString
    ));

var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(60)
};

// ================= TELEGRAM =================
var telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
if (string.IsNullOrWhiteSpace(telegramToken))
    throw new InvalidOperationException("TELEGRAM_BOT_TOKEN is not configured.");

builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(telegramToken, httpClient));

// ================= AUTOMAPPER =================
builder.Services.AddAutoMapper(typeof(UserMappingProfile));
builder.Services.AddAutoMapper(typeof(FamilyMappingProfile));
builder.Services.AddAutoMapper(typeof(Mappings));
builder.Services.AddAutoMapper(typeof(IncomeMappingProfile));
builder.Services.AddAutoMapper(typeof(ExpenseMappingProfile));
builder.Services.AddAutoMapper(typeof(GoalMappingProfile));
builder.Services.AddAutoMapper(typeof(SavingsContributionMappingProfile));
builder.Services.AddAutoMapper(typeof(SavingsWithdrawalMappingProfile));
builder.Services.AddAutoMapper(typeof(ReceiptMappingProfile));
builder.Services.AddAutoMapper(typeof(FamilyInviteMappingProfile));
builder.Services.AddAutoMapper(typeof(ReportSubscriptionMappingProfile));

// ================= REPOSITORIES =================
builder.Services.AddScoped<IFamilyRepository, FamilyRepository>();
builder.Services.AddScoped<IFamilyInviteRepository, FamilyInviteRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<ISavingsContributionRepository, SavingsContributionRepository>();
builder.Services.AddScoped<ISavingsWithdrawalRepository, SavingsWithdrawalRepository>();
builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
builder.Services.AddScoped<IReportSubscriptionRepository, ReportSubscriptionRepository>();
builder.Services.AddScoped<IFinanceWriter, FinanceWriter>();
builder.Services.AddScoped<IFamilyMembershipWriter, FamilyMembershipWriter>();

// ================= SERVICES =================
builder.Services.AddScoped<IFamilyService, FamilyService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddScoped<IFamilyInviteService, FamilyInviteService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<ISavingsService, SavingsService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IFamilyReportService, FamilyReportService>();
builder.Services.AddScoped<IFamilyHistoryService, FamilyHistoryService>();
builder.Services.AddScoped<IReportSubscriptionService, ReportSubscriptionService>();
builder.Services.AddScoped<IChatClient>(_ =>
{
    var baseUrl = Environment.GetEnvironmentVariable("OLLAMA_BASE_URL") ?? "http://localhost:11434/";
    var client = new HttpClient
    {
        BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/"),
        Timeout = TimeSpan.FromMinutes(5)
    };
    return new OllamaApiClient(client, Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "qwen3.5:4b", null);
});
builder.Services.AddScoped<IReceiptParser, OllamaReceiptParser>();

// ================= BOT =================
builder.Services.AddScoped<BotHandler>();
builder.Services.AddScoped<CommandRouter>();
builder.Services.AddScoped<CallbackRouter>();
builder.Services.AddScoped<MessageRouter>();

builder.Services.AddScoped<StartHandler>();
builder.Services.AddScoped<RegistrationHandler>();
builder.Services.AddScoped<ExpenseHandler>();
builder.Services.AddScoped<IncomeHandler>();
builder.Services.AddScoped<InitialBalanceHandler>();
builder.Services.AddScoped<ReportHandler>();
builder.Services.AddScoped<InviteHandler>();
builder.Services.AddScoped<FamilyHandler>();
builder.Services.AddScoped<SavingsHandler>();
builder.Services.AddScoped<ReceiptHandler>();
builder.Services.AddScoped<FamilyHistoryHandler>();
builder.Services.AddScoped<ReceiptHistoryHandler>();
builder.Services.AddScoped<ReportSubscriptionHandler>();

builder.Services.AddSingleton<ReceiptProcessingQueue>();
builder.Services.AddHostedService<ReceiptProcessingWorker>();
builder.Services.AddHostedService<ReportSubscriptionWorker>();

builder.Services.AddSingleton<UserStateService>();
builder.Services.AddSingleton<TempExpenseStorage>();
builder.Services.AddSingleton<TempIncomeStorage>();
builder.Services.AddSingleton<TempInviteStorage>();
builder.Services.AddSingleton<TempRegistrationStorage>();
builder.Services.AddSingleton<TempReportSubscriptionStorage>();

var app = builder.Build();

var bot = app.Services.GetRequiredService<ITelegramBotClient>();
if (bot is not TelegramBotClient pollingBot)
    throw new InvalidOperationException("Telegram bot client must support polling events.");

pollingBot.OnUpdate += async update =>
{
    await using var scope = app.Services.CreateAsyncScope();
    var handler = scope.ServiceProvider.GetRequiredService<BotHandler>();
    await handler.HandleUpdateAsync(bot, update);
};

pollingBot.OnError += async (exception, source) =>
{
    await using var scope = app.Services.CreateAsyncScope();
    var handler = scope.ServiceProvider.GetRequiredService<BotHandler>();
    await handler.HandleErrorAsync(bot, exception, CancellationToken.None);
};

var botInfo = await bot.GetMe();
Console.WriteLine($"Bot started: @{botInfo.Username}");
await app.RunAsync();

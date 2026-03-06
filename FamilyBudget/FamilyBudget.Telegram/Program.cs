using FamilyBudget.Application.Interfaces;
using FamilyBudget.Application.Services;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Infrastructure;
using FamilyBudget.Infrastructure.Mapper;
using FamilyBudget.Infrastructure.Repositories;
using FamilyBudget.Telegram.Bot;
using FamilyBudget.Telegram.Handlers;
using FamilyBudget.Telegram.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;

var builder = Host.CreateApplicationBuilder(args);

// ================= DATABASE =================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("FamilyBudgetDbContext")
    ));

var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(60)
};

// ================= TELEGRAM =================
builder.Services.AddSingleton<ITelegramBotClient>(
    new TelegramBotClient(builder.Configuration["Telegram:Token"], httpClient)
);

// ================= AUTOMAPPER =================
builder.Services.AddAutoMapper(typeof(UserMappingProfile));
builder.Services.AddAutoMapper(typeof(FamilyMappingProfile));
builder.Services.AddAutoMapper(typeof(CategoryMappingProfile));
builder.Services.AddAutoMapper(typeof(IncomeMappingProfile));
builder.Services.AddAutoMapper(typeof(ExpenseMappingProfile));
builder.Services.AddAutoMapper(typeof(GoalMappingProfile));

// ================= REPOSITORIES =================
builder.Services.AddScoped<IFamilyRepository, FamilyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
// ================= SERVICES =================
builder.Services.AddScoped<IFamilyService, FamilyService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<MonthlyReportService>();

// ================= BOT =================
builder.Services.AddScoped<BotHandler>();
builder.Services.AddScoped<CommandRouter>();
builder.Services.AddScoped<CallbackRouter>();
builder.Services.AddScoped<MessageRouter>();

builder.Services.AddScoped<StartHandler>();
builder.Services.AddScoped<RegistrationHandler>();
builder.Services.AddScoped<ExpenseHandler>();

builder.Services.AddSingleton<UserStateService>();
builder.Services.AddSingleton<TempExpenseStorage>();

var app = builder.Build();

var bot = app.Services.GetRequiredService<ITelegramBotClient>();
var handler = app.Services.GetRequiredService<BotHandler>();

bot.StartReceiving(
    handler.HandleUpdateAsync,
    handler.HandleErrorAsync,
    new ReceiverOptions(),
    CancellationToken.None);

Console.WriteLine("Bot started...");
Console.ReadLine();
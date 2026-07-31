using FamilyBudget.Application;
using FamilyBudget.Infrastructure;
using FamilyBudget.Infrastructure.Configuration;
using FamilyBudget.Persistence;
using FamilyBudget.Presentation.Telegram;

EnvironmentFileLoader.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var connectionString = builder.Configuration.GetConnectionString("FamilyBudgetDbContext");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings__FamilyBudgetDbContext is not configured.");

var telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
if (string.IsNullOrWhiteSpace(telegramToken))
    throw new InvalidOperationException("TELEGRAM_BOT_TOKEN is not configured.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddPersistence(connectionString);
builder.Services.AddTelegramBot(telegramToken);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

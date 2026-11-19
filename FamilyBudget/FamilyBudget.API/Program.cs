using FamilyBudget.Application;
using FamilyBudget.Infrastructure;
using FamilyBudget.Infrastructure.Configuration;
using FamilyBudget.Persistence;
using FamilyBudget.Presentation.Telegram;
using FamilyBudget.Infrastructure.Authentication;
using System.Text;
using FamilyBudget.API.Authentication;

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
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/api/auth/check", () => Results.NoContent())
    .RequireAuthorization();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

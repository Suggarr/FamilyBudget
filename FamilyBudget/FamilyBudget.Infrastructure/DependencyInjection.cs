using FamilyBudget.Core.Interfaces;
using FamilyBudget.Infrastructure.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace FamilyBudget.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IChatClient>(_ =>
        {
            var baseUrl = Environment.GetEnvironmentVariable("OLLAMA_BASE_URL")
                ?? "http://localhost:11434/";

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/"),
                Timeout = TimeSpan.FromMinutes(5)
            };

            var model = Environment.GetEnvironmentVariable("OLLAMA_MODEL");
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidOperationException(
                    "OLLAMA_MODEL environment variable is required.");
            }

            return new OllamaApiClient(client, model, null);
        });

        services.AddScoped<IReceiptParser, OllamaReceiptParser>();

        return services;
    }
}

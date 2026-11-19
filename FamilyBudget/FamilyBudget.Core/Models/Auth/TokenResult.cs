namespace FamilyBudget.Core.Models.Auth
{
    public sealed record TokenResult(string Token, DateTimeOffset ExpiresAtUtc);
}

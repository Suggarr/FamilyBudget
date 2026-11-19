using FamilyBudget.Core.Models.Auth;

namespace FamilyBudget.Core.Interfaces
{
    public interface ITokenProvider
    {
        TokenResult GenerateAccessToken(Guid accountId, Guid sessionId);
    }
}
namespace FamilyBudget.Application.Interfaces
{
    public interface IFamilyInviteService
    {
        Task<string> CreateInvite(Guid familyId);
        Task<bool> IsInviteValid(string code);
        Task<FamilyBudget.Core.Dtos.Family.JoinFamilyResult> JoinFamilyAsync(
            string code,
            long telegramId,
            string userName,
            string? telegramUsername);
    }
}

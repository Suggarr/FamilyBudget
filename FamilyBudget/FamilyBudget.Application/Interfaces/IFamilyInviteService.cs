namespace FamilyBudget.Application.Interfaces
{
    public interface IFamilyInviteService
    {
        Task<string> CreateInvite(Guid familyId);
        Task<Guid?> UseInvite(string code);
    }
}
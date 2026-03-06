namespace FamilyBudget.Application.Interfaces
{
    public interface IRegistrationService
    {
        Task<bool> IsRegisteredAsync(long telegramId);
        Task RegisterNewFamilyAsync(long telegramId, string familyName, string userName);
    }
}
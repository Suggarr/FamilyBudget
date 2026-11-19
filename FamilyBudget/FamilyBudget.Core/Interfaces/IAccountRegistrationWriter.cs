using FamilyBudget.Core.Models;
using FamilyBudget.Core.Models.Auth;

namespace FamilyBudget.Core.Interfaces
{
    public interface IAccountRegistrationWriter
    {
        Task CreateAsync(Account account, LocalCredential credential, User user, CancellationToken cancellationToken = default);
    }
}
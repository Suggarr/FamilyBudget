using FamilyBudget.Core.Models;
using FamilyBudget.Core.Models.Auth;

namespace FamilyBudget.Core.Interfaces;

public interface IFamilyMembershipWriter
{
    Task CreateFamilyAsync(
        Family family,
        User user,
        Account? newAccount,
        ExternalLogin externalLogin);

    Task JoinByInviteAsync(
        FamilyInvite invite,
        User user,
        Account? newAccount,
        ExternalLogin externalLogin);
}

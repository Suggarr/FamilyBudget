using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces;

public interface IFamilyMembershipWriter
{
    Task CreateFamilyAsync(Family family, User user, bool isNewUser);
    Task JoinByInviteAsync(FamilyInvite invite, User user, bool isNewUser);
}

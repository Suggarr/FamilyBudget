using FamilyBudget.Core.Models.Auth;

namespace FamilyBudget.Core.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(Account account, string password);
        bool Verify(Account account, string hash, string password);
    }
}
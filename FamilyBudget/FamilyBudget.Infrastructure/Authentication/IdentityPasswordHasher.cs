using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FamilyBudget.Infrastructure.Authentication
{
    public class IdentityPasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<Account> _hasher = new(Options.Create(new PasswordHasherOptions()));

        public string Hash(Account account, string password) =>
            _hasher.HashPassword(account, password);

        public bool Verify(Account account, string hash, string password) =>
            _hasher.VerifyHashedPassword(account, hash, password) != PasswordVerificationResult.Failed;
    }
}

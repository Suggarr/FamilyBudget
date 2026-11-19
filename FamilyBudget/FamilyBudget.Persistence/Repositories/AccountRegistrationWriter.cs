using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Core.Exceptions;
using FamilyBudget.Core.Models.Auth;
using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FamilyBudget.Persistence.Repositories
{
    public class AccountRegistrationWriter : IAccountRegistrationWriter
    {
        private readonly ApplicationDbContext _context;

        public AccountRegistrationWriter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Account account, LocalCredential credential, User user, CancellationToken cancellationToken = default)
        {
            if (account.Id != credential.AccountId || account.Id != user.Id)
            {
                throw new InvalidOperationException("Account, credential, and user must have the same Id.");
            }

            var accountEntity = new AccountEntity
            {
                Id = account.Id,
                DisplayName = account.DisplayName,
                CreatedAtUtc = account.CreatedAtUtc,
                DisabledAtUtc = account.DisabledAtUtc,

                LocalCredential = new LocalCredentialEntity
                {
                    AccountId = credential.AccountId,
                    Email = credential.Email,
                    NormalizedEmail = credential.NormalizedEmail,
                    PasswordHash = credential.PasswordHash,
                    CreatedAtUtc = credential.CreatedAtUtc,
                    PasswordChangedAtUtc = credential.PasswordChangedAtUtc,
                },

                User = new UserEntity
                {
                    Id = user.Id,
                    FamilyId = user.FamilyId,
                    Name = user.Name,
                    Balance = user.Balance,
                }
            };

            await _context.Accounts.AddAsync(accountEntity, cancellationToken);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: "IX_LocalCredentials_NormalizedEmail"
                })
            {
                _context.ChangeTracker.Clear();
                throw new EmailAlreadyRegisteredException();
            }

        }
    }
}

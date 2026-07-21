using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFamilyMembershipWriter _membershipWriter;

        public RegistrationService(
            IUserRepository userRepository,
            IFamilyMembershipWriter membershipWriter)
        {
            _userRepository = userRepository;
            _membershipWriter = membershipWriter;
        }

        public async Task<bool> IsRegisteredAsync(long telegramId)
        {
            var user = await _userRepository
                .GetByTelegramIdAsync(telegramId);

            return user != null;
        }

        public async Task RegisterNewFamilyAsync(
            long telegramId,
            string familyName,
            string userName,
            string? telegramUsername)
        {
            var existingUser = await _userRepository.GetByTelegramIdAsync(telegramId);
            if (existingUser?.FamilyId is not null)
                throw new InvalidOperationException("User already belongs to a family.");

            var familyResult = Family.Create(
                Guid.NewGuid(),
                familyName);

            if (familyResult.IsFailure)
                throw new Exception(familyResult.Error);

            var family = familyResult.Value;

            if (existingUser is not null)
            {
                var joinResult = existingUser.JoinFamily(family.Id, userName);
                if (joinResult.IsFailure)
                    throw new InvalidOperationException(joinResult.Error);

                var usernameResult = existingUser.SetTelegramUsername(telegramUsername);
                if (usernameResult.IsFailure)
                    throw new InvalidOperationException(usernameResult.Error);

                await _membershipWriter.CreateFamilyAsync(family, existingUser, false);
                return;
            }

            var userResult = User.Create(
                Guid.NewGuid(),
                family.Id,
                userName,
                telegramId,
                telegramUsername: telegramUsername);

            if (userResult.IsFailure)
                throw new Exception(userResult.Error);

            await _membershipWriter.CreateFamilyAsync(family, userResult.Value, true);
        }
    }
}

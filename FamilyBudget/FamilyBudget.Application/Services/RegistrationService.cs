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
        private readonly IFamilyRepository _familyRepository;

        public RegistrationService(
            IUserRepository userRepository,
            IFamilyRepository familyRepository)
        {
            _userRepository = userRepository;
            _familyRepository = familyRepository;
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
            string userName)
        {
            var familyResult = Family.Create(
                Guid.NewGuid(),
                familyName);

            if (familyResult.IsFailure)
                throw new Exception(familyResult.Error);

            var family = familyResult.Value;
            await _familyRepository.AddAsync(family);

            var userResult = User.Create(
                Guid.NewGuid(),
                family.Id,
                userName,
                telegramId);

            if (userResult.IsFailure)
                throw new Exception(userResult.Error);

            var user = userResult.Value;
            await _userRepository.AddAsync(user);
        }
    }
}

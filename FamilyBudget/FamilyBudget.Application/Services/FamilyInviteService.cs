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
    public class FamilyInviteService : IFamilyInviteService
    {
        private readonly IFamilyInviteRepository _familyInviteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFamilyMembershipWriter _membershipWriter;

        public FamilyInviteService(
            IFamilyInviteRepository familyInviteRepository,
            IUserRepository userRepository,
            IFamilyMembershipWriter membershipWriter)
        {
            _familyInviteRepository = familyInviteRepository;
            _userRepository = userRepository;
            _membershipWriter = membershipWriter;
        }

        public async Task<string> CreateInvite(Guid familyId)
        {
            var invite = FamilyInvite.Create(
                Guid.NewGuid(),
                familyId,
                "",
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(24))
                .Value;

            invite.GenerateCode();

            invite.SetLifeTime(TimeSpan.FromHours(24));

            await _familyInviteRepository.AddAsync(invite);

            return invite.Code;
        }

        public async Task<bool> IsInviteValid(string code)
        {
            var invite = await _familyInviteRepository.GetByCodeAsync(code);

            return invite is not null && !invite.IsExpired;
        }

        public async Task<FamilyBudget.Core.Dtos.Family.JoinFamilyResult> JoinFamilyAsync(
            string code,
            long telegramId,
            string userName,
            string? telegramUsername)
        {
            var invite = await _familyInviteRepository.GetByCodeAsync(code);
            if (invite is null || invite.IsExpired)
                return FamilyBudget.Core.Dtos.Family.JoinFamilyResult.Failure("Код приглашения недействителен или истёк.");

            var user = await _userRepository.GetByTelegramIdAsync(telegramId);
            var isNewUser = user is null;
            if (user is not null && user.FamilyId.HasValue && user.FamilyId != invite.FamilyId)
                return FamilyBudget.Core.Dtos.Family.JoinFamilyResult.Failure("Сначала покиньте текущую семью.");

            if (user is null)
            {
                var createResult = User.Create(
                    Guid.NewGuid(),
                    invite.FamilyId,
                    userName,
                    telegramId,
                    telegramUsername: telegramUsername);
                if (createResult.IsFailure)
                    return FamilyBudget.Core.Dtos.Family.JoinFamilyResult.Failure(createResult.Error);

                user = createResult.Value;
            }
            else
            {
                var joinResult = user.JoinFamily(invite.FamilyId, userName);
                if (joinResult.IsFailure)
                    return FamilyBudget.Core.Dtos.Family.JoinFamilyResult.Failure(joinResult.Error);

                var usernameResult = user.SetTelegramUsername(telegramUsername);
                if (usernameResult.IsFailure)
                    return FamilyBudget.Core.Dtos.Family.JoinFamilyResult.Failure(usernameResult.Error);

            }

            try
            {
                await _membershipWriter.JoinByInviteAsync(invite, user, isNewUser);
            }
            catch (InvalidOperationException ex) when (ex.Message == "Invite has already been used or expired.")
            {
                return FamilyBudget.Core.Dtos.Family.JoinFamilyResult.Failure("Код приглашения уже использован или истёк.");
            }

            return FamilyBudget.Core.Dtos.Family.JoinFamilyResult.Success();
        }
    }
}

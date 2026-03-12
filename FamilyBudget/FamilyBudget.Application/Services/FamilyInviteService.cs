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

        public FamilyInviteService(IFamilyInviteRepository familyInviteRepository)
        {
            _familyInviteRepository = familyInviteRepository;
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

        public async Task<Guid?> UseInvite(string code)
        {
            var invite = await _familyInviteRepository.GetByCodeAsync(code);

            if (invite == null)
                return null;

            if (invite.IsExpired)
                return null;

            await _familyInviteRepository.DeleteAsync(invite.Id);

            return invite.FamilyId;
        }
    }
}

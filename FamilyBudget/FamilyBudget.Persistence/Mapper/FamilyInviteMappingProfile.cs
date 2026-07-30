using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;

namespace FamilyBudget.Persistence.Mapper
{
    public class FamilyInviteMappingProfile : Profile
    {
        public FamilyInviteMappingProfile()
        {
            CreateMap<FamilyInviteEntity, FamilyInvite>()
                .ConstructUsing(f => FamilyInvite.Create(f.Id, f.FamilyId, f.Code, f.CreatedAt, f.ExpiresAt).Value);

            CreateMap<FamilyInvite, FamilyInviteEntity>();
        }
    }
}

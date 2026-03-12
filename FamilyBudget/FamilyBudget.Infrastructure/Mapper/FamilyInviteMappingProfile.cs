using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Mapper
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

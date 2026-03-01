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
    public class FamilyMappingProfile : Profile
    {
        public FamilyMappingProfile()
        {
            CreateMap<FamilyEntity, Family>()
                .ConstructUsing(f => Family.Create(f.Id, f.Name).Value);

            CreateMap<Family, FamilyEntity>();
        }
    }
}

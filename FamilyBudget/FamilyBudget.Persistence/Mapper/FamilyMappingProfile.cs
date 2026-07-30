using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;

namespace FamilyBudget.Persistence.Mapper
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

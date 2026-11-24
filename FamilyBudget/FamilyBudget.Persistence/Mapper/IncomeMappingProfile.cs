using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;

namespace FamilyBudget.Persistence.Mapper
{
    public class IncomeMappingProfile : Profile
    {
        public IncomeMappingProfile()
        {
            CreateMap<IncomeEntity, Income>()
                .ConstructUsing(i => Income.Create(i.Id, i.FamilyId, i.UserId, i.Amount, i.Source, i.Date).Value);
            CreateMap<Income, IncomeEntity>()
                .ForMember(entity => entity.Family, options => options.Ignore())
                .ForMember(entity => entity.User, options => options.Ignore());
        }
    }
}

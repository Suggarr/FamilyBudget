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
            CreateMap<Income, IncomeEntity>();
        }
    }
}

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

            CreateMap<Family, FamilyEntity>()
                .ForMember(entity => entity.Users, options => options.Ignore())
                .ForMember(entity => entity.Expenses, options => options.Ignore())
                .ForMember(entity => entity.Incomes, options => options.Ignore())
                .ForMember(entity => entity.Goals, options => options.Ignore())
                .ForMember(entity => entity.FamilyInvites, options => options.Ignore())
                .ForMember(entity => entity.SavingsContributions, options => options.Ignore())
                .ForMember(entity => entity.SavingsWithdrawals, options => options.Ignore())
                .ForMember(entity => entity.Receipts, options => options.Ignore());
        }
    }
}

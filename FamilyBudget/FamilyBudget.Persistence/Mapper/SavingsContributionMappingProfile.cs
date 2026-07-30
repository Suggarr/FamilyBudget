using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;

namespace FamilyBudget.Persistence.Mapper;

public class SavingsContributionMappingProfile : Profile
{
    public SavingsContributionMappingProfile()
    {
        CreateMap<SavingsContributionEntity, SavingsContribution>()
            .ConstructUsing(c => SavingsContribution.Create(c.Id, c.FamilyId, c.UserId, c.Amount, c.CreatedAt).Value);
        CreateMap<SavingsContribution, SavingsContributionEntity>();
    }
}

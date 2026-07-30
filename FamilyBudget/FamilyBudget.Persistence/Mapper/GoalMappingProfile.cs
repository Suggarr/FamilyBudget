using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;

namespace FamilyBudget.Persistence.Mapper
{
    public class GoalMappingProfile : Profile
    {
        public GoalMappingProfile()
        {
            CreateMap<GoalEntity, Goal>()
                .ConstructUsing(g => Goal.Create(g.Id, g.FamilyId, g.Title, g.TargetAmount, g.CurrentAmount).Value);
            CreateMap<Goal, GoalEntity>();
        }
    }
}

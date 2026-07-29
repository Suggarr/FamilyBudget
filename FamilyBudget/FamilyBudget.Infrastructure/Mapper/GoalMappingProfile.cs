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

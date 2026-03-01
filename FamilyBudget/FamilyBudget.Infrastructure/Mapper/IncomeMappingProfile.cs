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

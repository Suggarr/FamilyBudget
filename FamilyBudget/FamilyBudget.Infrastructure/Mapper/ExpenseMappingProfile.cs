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
    public class ExpenseMappingProfile : Profile
    {
        public ExpenseMappingProfile()
        {
            CreateMap<ExpenseEntity, Expense>()
                .ConstructUsing(e => Expense.Create(e.Id, e.FamilyId, e.UserId, e.CategoryId, e.Amount, e.Description, e.Date).Value);
            CreateMap<Expense, ExpenseEntity>();
        }
    }
}

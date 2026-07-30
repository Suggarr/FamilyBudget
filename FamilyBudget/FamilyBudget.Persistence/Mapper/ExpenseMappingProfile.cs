using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;

namespace FamilyBudget.Persistence.Mapper
{
    public class ExpenseMappingProfile : Profile
    {
        public ExpenseMappingProfile()
        {
            CreateMap<ExpenseEntity, Expense>()
                .ConstructUsing(e => Expense.Create(e.Id, e.FamilyId, e.UserId, e.Category, e.Amount, e.Description, e.Date).Value);
            CreateMap<Expense, ExpenseEntity>();
        }
    }
}

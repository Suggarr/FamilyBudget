using AutoMapper;
using FamilyBudget.Application.Dtos.Income;
using FamilyBudget.Core.Dtos.Expense;
using FamilyBudget.Core.Dtos.User;
using FamilyBudget.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application
{
    public class Mappings : Profile
    {
        public Mappings()
        {
            CreateMap<Expense, ExpenseDto>();
            CreateMap<Income, IncomeDto>();
            CreateMap<User, UserDto>();
        }
    }
}

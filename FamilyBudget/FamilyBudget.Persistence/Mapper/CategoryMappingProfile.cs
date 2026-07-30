//using AutoMapper;
//using FamilyBudget.Core.Models;
//using FamilyBudget.Persistence.Entities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FamilyBudget.Persistence.Mapper
//{
//    public class CategoryMappingProfile : Profile
//    {
//        public CategoryMappingProfile()
//        {
//            CreateMap<CategoryEntity, Category>()
//                .ConstructUsing(c => Category.Create(c.Id, c.FamilyId, c.Name).Value);
//            CreateMap<Category, CategoryEntity>();
//        }
//    }
//}

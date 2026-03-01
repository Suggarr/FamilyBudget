//using AutoMapper;
//using FamilyBudget.Core.Interfaces;
//using FamilyBudget.Core.Models;
//using FamilyBudget.Infrastructure.Entities;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FamilyBudget.Infrastructure.Repositories
//{
//    public class CategoryRepository 
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IMapper _mapper;

//        public CategoryRepository(ApplicationDbContext context, IMapper mapper)
//        {
//            _context = context;
//            _mapper = mapper;
//        }

//        public async Task<Guid> AddAsync(Category category)
//        {
//            var categoryEntity = _mapper.Map<CategoryEntity>(category);

//            await _context.Categories.AddAsync(categoryEntity);
//            await _context.SaveChangesAsync();

//            return category.Id;
//        }

//        public async Task<Category?> GetByNameAsync(Guid familyId, string name)
//        {
//            var categoryEntity = await _context.Categories.FirstOrDefaultAsync(c => c.FamilyId == familyId && c.Name == name);

//            return categoryEntity == null
//                ? null
//                : _mapper.Map<Category>(categoryEntity);
//        }

//        public async Task<Guid> DeleteAsync(Guid id)
//        {
//            var categoryEntity = await _context.Categories.FindAsync(id);
//            if (categoryEntity != null)
//            {
//                _context.Categories.Remove(categoryEntity);
//                await _context.SaveChangesAsync();
//            }
//            return id;
//        }

//        public async Task<Guid> UpdateAsync(Guid id, string name)
//        {
//            await _context.Families
//                .Where(f => f.Id == id)
//                .ExecuteUpdateAsync(setters => setters
//                    .SetProperty(f => f.Name, name));

//            return id;
//        }
//    }
//}

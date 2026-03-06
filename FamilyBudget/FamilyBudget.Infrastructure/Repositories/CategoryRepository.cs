using AutoMapper;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Guid> AddAsync(Category category)
        {
            var categoryEntity = _mapper.Map<CategoryEntity>(category);

            await _context.Categories.AddAsync(categoryEntity);
            await _context.SaveChangesAsync();

            return category.Id;
        }

        public async Task<Category?> GetByNameAsync(Guid familyId, string name)
        {
            var categoryEntity = await _context.Categories.FirstOrDefaultAsync(c => c.FamilyId == familyId && c.Name == name);

            return categoryEntity == null
                ? null
                : _mapper.Map<Category>(categoryEntity);
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            var categoryEntity = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            return categoryEntity == null
                ? null
                : _mapper.Map<Category>(categoryEntity);
        }

        public async Task<List<Category>> GetByFamilyAsync(Guid familyId)
        {
            var categoryEntities = await _context.Categories
                .AsNoTracking()
                .Where(c => c.FamilyId == familyId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return _mapper.Map<List<Category>>(categoryEntities);
        }

        public async Task<bool> ExistsAsync(Guid familyId, string name)
        {
            return await _context.Categories
                .AnyAsync(c => c.FamilyId == familyId && c.Name == name);
        }

        public async Task<Guid> DeleteAsync(Guid id)
        {
            var categoryEntity = await _context.Categories.FindAsync(id);
            if (categoryEntity != null)
            {
                _context.Categories.Remove(categoryEntity);
                await _context.SaveChangesAsync();
            }
            return id;
        }

        public async Task UpdateAsync(Category category)
        {
            var entity = _mapper.Map<CategoryEntity>(category);

            _context.Categories.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}

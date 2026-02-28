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
    public class FamilyRepositories
    {
        private readonly ApplicationDbContext _context;

        public FamilyRepositories(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Family>> GetAllAsync()
        {
            var familyEntities = await _context.Families
                .AsNoTracking()
                .ToListAsync();

            return familyEntities.Select(f => Family.Create(f.Id, f.Name).Value).ToList();
        }

        public async Task<Family?> GetByIdAsync(Guid id)
        {
            var familyEntity = await _context.Families.FindAsync(id);

            return familyEntity == null ? null : Family.Create(familyEntity.Id, familyEntity.Name).Value;
        }

        public async Task<Family?> GetByNameAsync(string name)
        {
            var familyEntity = await _context.Families.FirstOrDefaultAsync(f => f.Name == name);

            return familyEntity == null ? null : Family.Create(familyEntity.Id, familyEntity.Name).Value;
        }

        public async Task<Guid> AddAsync(Family family)
        {
            var familyEntity = new FamilyEntity
            {
                Id = family.Id,
                Name = family.Name
            };
            await _context.Families.AddAsync(familyEntity);
            await _context.SaveChangesAsync();

            return family.Id;
        }

        public async Task<Guid> DeleteAsync(Guid id)
        {
            var familyEntity = await _context.Families.FindAsync(id);
            if (familyEntity != null)
            {
                _context.Families.Remove(familyEntity);
                await _context.SaveChangesAsync();
            }
            return id;
        }

        public async Task<Guid> UpdateAsync(Guid id, string name)
        {
            await _context.Families
                .Where(f => f.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(f => f.Name, name));

            return id;
        }
    }
}

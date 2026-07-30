using AutoMapper;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Persistence.Repositories
{
    public class FamilyRepository : IFamilyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public FamilyRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<Family>> GetAllAsync()
        {
            var familyEntities = await _context.Families
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<Family>>(familyEntities);
        }

        public async Task<Family?> GetByIdAsync(Guid id)
        {
            var familyEntity = await _context.Families.FindAsync(id);

            return familyEntity == null ? null : _mapper.Map<Family>(familyEntity);
        }

        public async Task<Family?> GetByNameAsync(string name)
        {
            var familyEntity = await _context.Families.FirstOrDefaultAsync(f => f.Name == name);

            return familyEntity == null ? null : _mapper.Map<Family>(familyEntity);
        }

        public async Task<Guid> AddAsync(Family family)
        {
            var familyEntity = _mapper.Map<FamilyEntity>(family);

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

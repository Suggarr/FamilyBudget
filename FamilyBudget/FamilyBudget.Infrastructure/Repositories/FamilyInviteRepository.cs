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
    public class FamilyInviteRepository : IFamilyInviteRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public FamilyInviteRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddAsync(FamilyInvite invite)
        {
            var entity = _mapper.Map<FamilyInviteEntity>(invite);

            await _context.FamilyInvites.AddAsync(entity);

            await _context.SaveChangesAsync();
        }

        public async Task<FamilyInvite?> GetByCodeAsync(string code)
        {
            var entity = await _context.FamilyInvites
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code);

            if (entity == null)
                return null;

            return _mapper.Map<FamilyInvite>(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.FamilyInvites
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return;

            _context.FamilyInvites.Remove(entity);

            await _context.SaveChangesAsync();
        }
    }
}

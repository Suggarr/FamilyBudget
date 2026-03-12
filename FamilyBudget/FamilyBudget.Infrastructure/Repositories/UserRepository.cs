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
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<User?> GetByTelegramIdAsync(long telegramId)
        {
            var userEntity = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.TelegramId == telegramId);

            return userEntity == null
                ? null
                : _mapper.Map<User>(userEntity);
        }

        public async Task<List<User>> GetByFamilyIdAsync(Guid familyId)
        {
            var userEntities = await _context.Users
                .AsNoTracking()
                .Where(u => u.FamilyId == familyId)
                .ToListAsync();


            return _mapper.Map<List<User>>(userEntities);
        }

        public async Task<Guid> AddAsync(User user)
        {
            var userEntity = _mapper.Map<UserEntity>(user);

            await _context.Users.AddAsync(userEntity);
            await _context.SaveChangesAsync();

            return user.Id;
        }

        public async Task<Guid> DeleteAsync(Guid id)
        {
            var userEntity = await _context.Users.FindAsync(id);
            if (userEntity != null)
            {
                _context.Users.Remove(userEntity);
                await _context.SaveChangesAsync();
            }
            return id;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(_mapper.Map<UserEntity>(user));
            await _context.SaveChangesAsync();
        }
    }
}

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
    public class UserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByTelegramIdAsync(long telegramId)
        {
            var userEntity = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.TelegramId == telegramId);

            return userEntity == null 
                ? null
                : User.Create(userEntity.Id, userEntity.FamilyId, userEntity.Name, userEntity.TelegramId).Value;
        }

        public async Task<List<User>> GetByFamilyIdAsync(Guid familyId)
        {
            var userEntities = await _context.Users
                .AsNoTracking()
                .Where(u => u.FamilyId == familyId)
                .ToListAsync();


            return userEntities
                .Select(u => User.Create(u.Id, u.FamilyId, u.Name, u.TelegramId).Value)
                .ToList();
        }

        public async Task<Guid> AddAsync(User user)
        {
            var userEntity = new UserEntity
            {
                Id = user.Id,
                FamilyId = user.FamilyId,
                Name = user.Name,
                TelegramId = user.TelegramId
            };

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

        public async Task<Guid> UpdateAsync(Guid id, string name)
        {
            await _context.Users
                .Where(u => u.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.Name, name));

            return id;
        }
    }
}

using AutoMapper;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Dtos.User;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Guid> AddUserAsync(CreateUserDto createUserDto)
        {
            var userResult = User.Create(
                Guid.NewGuid(),
                createUserDto.FamilyId,
                createUserDto.Name,
                createUserDto.TelegramId
            );

            if (userResult.IsFailure)
            {
                throw new Exception(userResult.Error);
            }

            var user = userResult.Value;
            return await _userRepository.AddAsync(user);
        }

        public async Task<UserDto?> GetByTelegramIdAsync(long telegramId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);

            return user is null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<List<UserDto>> GetByFamilyIdAsync(Guid familyId)
        {
            var users = await _userRepository.GetByFamilyIdAsync(familyId);
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _userRepository.DeleteAsync(id);
        }

        public async Task LeaveFamily(long telegramId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);

            if (user == null)
                return;

            user.LeaveFamily();

            await _userRepository.UpdateAsync(user);
        }

        public async Task SetBalanceAsync(long telegramId, decimal balance)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId)
                ?? throw new InvalidOperationException("User not found.");

            var result = user.SetBalance(balance);
            if (result.IsFailure)
                throw new InvalidOperationException(result.Error);

            await _userRepository.UpdateAsync(user);
        }

        public async Task UpdateTelegramUsernameAsync(long telegramId, string? telegramUsername)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);
            if (user is null || user.TelegramUsername == telegramUsername?.Trim().TrimStart('@'))
                return;

            var result = user.SetTelegramUsername(telegramUsername);
            if (result.IsFailure)
                throw new InvalidOperationException(result.Error);

            await _userRepository.UpdateAsync(user);
        }
    }
}

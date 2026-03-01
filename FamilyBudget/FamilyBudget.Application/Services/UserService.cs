using AutoMapper;
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
    public class UserService
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
    }
}

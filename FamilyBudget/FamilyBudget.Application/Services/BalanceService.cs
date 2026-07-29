using FamilyBudget.Application.Interfaces;

using FamilyBudget.Core.Interfaces;

namespace FamilyBudget.Application.Services
{
    public class BalanceService
    {
        private readonly IUserRepository _userRepository;

        public BalanceService(
            IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Получает сохранённый текущий баланс личного счёта пользователя
        /// </summary>
        public async Task<decimal> GetUserBalanceAsync(Guid userId, Guid familyId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null || user.FamilyId != familyId)
                throw new InvalidOperationException("User not found in this family.");

            return user.Balance;
        }

        /// <summary>
        /// Получает сумму текущих остатков на личных счетах участников семьи
        /// </summary>
        public async Task<decimal> GetFamilyBalanceAsync(Guid familyId)
        {
            var users = await _userRepository.GetByFamilyIdAsync(familyId);
            return users.Sum(u => u.Balance);
        }
    }
}


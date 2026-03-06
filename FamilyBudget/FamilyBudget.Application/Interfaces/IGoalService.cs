using FamilyBudget.Application.Dtos.Goal;

namespace FamilyBudget.Application.Interfaces
{
    public interface IGoalService
    {
        Task AddMoneyAsync(Guid id, decimal amount);
        Task<Guid> CreateAsync(CreateGoalDto dto);
        Task DeleteAsync(Guid id);
        Task<List<GoalDto>> GetByFamilyAsync(Guid familyId);
    }
}
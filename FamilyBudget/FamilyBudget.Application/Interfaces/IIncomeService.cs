using FamilyBudget.Application.Dtos.Income;

namespace FamilyBudget.Application.Interfaces
{
    public interface IIncomeService
    {
        Task<Guid> AddAsync(CreateIncomeDto dto);
        Task DeleteAsync(Guid id);
        Task<List<IncomeDto>> GetByFamilyAsync(Guid familyId);
        Task<List<IncomeDto>> GetByPeriodAsync(Guid familyId, DateTime from, DateTime to);
    }
}

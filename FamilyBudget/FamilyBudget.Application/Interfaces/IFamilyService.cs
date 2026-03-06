using FamilyBudget.Core.Dtos.Family;
using FamilyBudget.Core.Dtos.User;

namespace FamilyBudget.Application.Interfaces
{
    public interface IFamilyService
    {
        Task<Guid> CreateAsync(CreateUserDto dto);
        Task DeleteAsync(Guid id);
        Task<List<FamilyDto>> GetAllAsync();
        Task<FamilyDto?> GetByIdAsync(Guid id);
    }
}
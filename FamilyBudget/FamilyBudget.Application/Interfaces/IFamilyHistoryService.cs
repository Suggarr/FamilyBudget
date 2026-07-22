using FamilyBudget.Application.Dtos.History;

namespace FamilyBudget.Application.Interfaces;

public interface IFamilyHistoryService
{
    public Task<FamilyHistoryPageDto> GetFamilyHistoryAsync(Guid familyId, int page, int pageSize = 10);
}
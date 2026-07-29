using FamilyBudget.Application.Dtos;

namespace FamilyBudget.Application.Interfaces
{
    public interface IFamilyReportService
    {
        Task<FamilyReportDto> GetFamilyReportAsync(Guid familyId, DateTime? startDate = null, DateTime? endDate = null);
    }
}

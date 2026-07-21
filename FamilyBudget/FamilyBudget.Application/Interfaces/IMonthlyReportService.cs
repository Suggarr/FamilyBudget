using FamilyBudget.Application.Dtos;

namespace FamilyBudget.Application.Interfaces
{
    public interface IMonthlyReportService
    {
        Task<MonthlyReportDto> GetFamilyMonthlyReportAsync(Guid familyId, int year, int month);
    }
}
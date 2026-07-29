using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces
{
    public interface IReportSubscriptionRepository
    {
        Task AddAsync(ReportSubscription reportSubscription);
        Task<ReportSubscription?> GetByUserIdAsync(Guid userId);
        Task<List<ReportSubscription>> GetDueAsync(DateTime utcNow);
        Task UpdateAsync(ReportSubscription reportSubscription);
    }
}
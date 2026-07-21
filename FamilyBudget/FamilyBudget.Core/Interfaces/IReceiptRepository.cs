using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces;

public interface IReceiptRepository
{
    Task<Guid> AddAsync(Receipt receipt);
    Task<Receipt?> GetByIdAsync(Guid id);
    Task<List<Receipt>> GetByFamilyIdAsync(Guid familyId);
    Task UpdateAsync(Receipt receipt);
}

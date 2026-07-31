using AutoMapper;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Persistence.Repositories;

public class ReceiptRepository : IReceiptRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ReceiptRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Guid> AddAsync(Receipt receipt)
    {
        await _context.Receipts.AddAsync(_mapper.Map<ReceiptEntity>(receipt));
        await _context.SaveChangesAsync();
        return receipt.Id;
    }

    public async Task<Receipt?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Receipts.Include(r => r.Items).AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return entity is null ? null : _mapper.Map<Receipt>(entity);
    }

    public async Task<List<Receipt>> GetByFamilyIdAsync(Guid familyId)
    {
        var entities = await _context.Receipts.Include(r => r.Items).AsNoTracking().Where(r => r.FamilyId == familyId).OrderByDescending(r => r.PurchasedAt).ToListAsync();
        return _mapper.Map<List<Receipt>>(entities);
    }

    public async Task UpdateAsync(Receipt receipt)
    {
        await _context.Receipts
            .Where(r => r.Id == receipt.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.Status, receipt.Status)
                .SetProperty(r => r.ExpenseId, receipt.ExpenseId));
    }
}

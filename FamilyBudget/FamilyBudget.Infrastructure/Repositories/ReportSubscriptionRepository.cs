using AutoMapper;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Infrastructure.Repositories
{
    public class ReportSubscriptionRepository : IReportSubscriptionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ReportSubscriptionRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ReportSubscription?> GetByUserIdAsync(Guid userId)
        {
            var entity = await _context.ReportSubscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UserId == userId);

            return entity == null ? null : _mapper.Map<ReportSubscription>(entity);
        }

        public async Task<List<ReportSubscription>> GetDueAsync(DateTime utcNow)
        {
            if (utcNow.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("DateTime must be in UTC", nameof(utcNow));
            }

            var entities = await _context.ReportSubscriptions
                .Where(e => e.IsEnabled && e.NextRunAt <= utcNow)
                .OrderBy(e => e.NextRunAt)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<ReportSubscription>>(entities);
        }

        public async Task AddAsync(ReportSubscription reportSubscription)
        {
            var entity = _mapper.Map<ReportSubscriptionEntity>(reportSubscription);

            await _context.ReportSubscriptions.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ReportSubscription reportSubscription)
        {
            var entity = _mapper.Map<ReportSubscriptionEntity>(reportSubscription);

            await _context.ReportSubscriptions
                .Where(e => e.Id == entity.Id)
                .ExecuteUpdateAsync(e => e.SetProperty(p => p.IsEnabled, entity.IsEnabled)
                    .SetProperty(p => p.DayOfWeek, entity.DayOfWeek)
                    .SetProperty(p => p.TimeOfDay, entity.TimeOfDay)
                    .SetProperty(p => p.NextRunAt, entity.NextRunAt));
        }
    }
}

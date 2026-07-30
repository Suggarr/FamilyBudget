using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;

namespace FamilyBudget.Persistence.Mapper
{
    public class ReportSubscriptionMappingProfile : Profile
    {
        public ReportSubscriptionMappingProfile()
        {
            CreateMap<ReportSubscriptionEntity, ReportSubscription>()
                .ConstructUsing(rs => ReportSubscription.Create(rs.Id, rs.UserId, rs.IsEnabled, rs.DayOfWeek, rs.TimeOfDay, rs.NextRunAt).Value);

            CreateMap<ReportSubscription, ReportSubscriptionEntity>();
        }
    }
}

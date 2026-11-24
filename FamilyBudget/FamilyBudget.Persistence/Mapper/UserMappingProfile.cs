using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;

namespace FamilyBudget.Persistence.Mapper
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<UserEntity, User>()
                .ConstructUsing(u => User.Create(u.Id, u.FamilyId, u.Name, u.Balance).Value);

            CreateMap<User, UserEntity>()
                .ForMember(entity => entity.Account, options => options.Ignore())
                .ForMember(entity => entity.Family, options => options.Ignore())
                .ForMember(entity => entity.Expenses, options => options.Ignore())
                .ForMember(entity => entity.Incomes, options => options.Ignore())
                .ForMember(entity => entity.SavingsContributions, options => options.Ignore())
                .ForMember(entity => entity.SavingsWithdrawals, options => options.Ignore())
                .ForMember(entity => entity.Receipts, options => options.Ignore())
                .ForMember(entity => entity.ReportSubscription, options => options.Ignore());
        }
    }
}

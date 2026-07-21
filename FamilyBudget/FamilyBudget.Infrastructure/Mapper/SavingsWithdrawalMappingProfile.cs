using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Infrastructure.Entities;

namespace FamilyBudget.Infrastructure.Mapper;

public class SavingsWithdrawalMappingProfile : Profile
{
    public SavingsWithdrawalMappingProfile()
    {
        CreateMap<SavingsWithdrawalEntity, SavingsWithdrawal>()
            .ConstructUsing(w => SavingsWithdrawal.Create(w.Id, w.FamilyId, w.UserId, w.Amount, w.CreatedAt).Value);
        CreateMap<SavingsWithdrawal, SavingsWithdrawalEntity>();
    }
}

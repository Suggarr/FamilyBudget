using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;

namespace FamilyBudget.Persistence.Mapper;

public class SavingsWithdrawalMappingProfile : Profile
{
    public SavingsWithdrawalMappingProfile()
    {
        CreateMap<SavingsWithdrawalEntity, SavingsWithdrawal>()
            .ConstructUsing(w => SavingsWithdrawal.Create(w.Id, w.FamilyId, w.UserId, w.Amount, w.CreatedAt).Value);
        CreateMap<SavingsWithdrawal, SavingsWithdrawalEntity>();
    }
}

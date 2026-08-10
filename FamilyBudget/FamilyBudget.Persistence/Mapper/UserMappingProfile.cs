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

            CreateMap<User, UserEntity>();
        }
    }
}

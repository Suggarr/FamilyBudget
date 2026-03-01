using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Infrastructure.Entities;

namespace FamilyBudget.Infrastructure.Mapper
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<UserEntity, User>()
                .ConstructUsing(u => User.Create(u.Id, u.FamilyId, u.Name, u.TelegramId).Value);

            CreateMap<User, UserEntity>();
        }
    }
}

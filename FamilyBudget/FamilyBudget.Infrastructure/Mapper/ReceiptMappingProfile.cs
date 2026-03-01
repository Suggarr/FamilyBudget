using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Mapper
{
    public class ReceiptMappingProfile : Profile
    {
        public ReceiptMappingProfile()
        {
            CreateMap<ReceiptEntity, Receipt>()
                .ConstructUsing(r => Receipt.Create(r.Id, r.FamilyId, r.UserId, r.FilePath, r.IsProcessed, r.TotalAmount).Value);
            CreateMap<Receipt, ReceiptEntity>();
        }
    }
}

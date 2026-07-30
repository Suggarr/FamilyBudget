using AutoMapper;
using FamilyBudget.Core.Models;
using FamilyBudget.Persistence.Entities;

namespace FamilyBudget.Persistence.Mapper;

public class ReceiptMappingProfile : Profile
{
    public ReceiptMappingProfile()
    {
        CreateMap<ReceiptEntity, Receipt>()
            .ConstructUsing(r => Receipt.Create(
                r.Id, r.FamilyId, r.UserId, r.MerchantName, r.PurchasedAt, r.Subtotal, r.DiscountAmount,
                r.TaxAmount, r.TotalAmount, r.Currency, r.SourceFileId, r.RawResponse,
                r.Items.Select(i => ReceiptItem.Create(i.Id, i.Name, i.Quantity, i.UnitPrice, i.DiscountAmount, i.TotalAmount).Value),
                r.ExpenseId,
                r.Status).Value);
        CreateMap<ReceiptItemEntity, ReceiptItem>().ConstructUsing(i => ReceiptItem.Create(i.Id, i.Name, i.Quantity, i.UnitPrice, i.DiscountAmount, i.TotalAmount).Value);
        CreateMap<Receipt, ReceiptEntity>();
        CreateMap<ReceiptItem, ReceiptItemEntity>();
    }
}

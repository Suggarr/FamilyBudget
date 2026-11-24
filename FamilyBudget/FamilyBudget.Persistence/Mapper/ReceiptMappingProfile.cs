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
        CreateMap<Receipt, ReceiptEntity>()
            .ForMember(entity => entity.Family, options => options.Ignore())
            .ForMember(entity => entity.User, options => options.Ignore())
            .ForMember(entity => entity.Expense, options => options.Ignore());
        CreateMap<ReceiptItem, ReceiptItemEntity>()
            // EF sets the parent FK when the item is added through ReceiptEntity.Items.
            .ForMember(entity => entity.ReceiptId, options => options.Ignore())
            .ForMember(entity => entity.Receipt, options => options.Ignore());
    }
}

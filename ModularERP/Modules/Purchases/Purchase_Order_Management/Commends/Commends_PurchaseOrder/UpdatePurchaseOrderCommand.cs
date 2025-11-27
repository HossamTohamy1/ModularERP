using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder
{
    public class UpdatePurchaseOrderCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public string CurrencyCode { get; set; } = "SAR";
        public DateTime PODate { get; set; }
        public Guid? PaymentTermId { get; set; }
        public string? Notes { get; set; }
        public string? Terms { get; set; }

        public List<UpdatePOLineItemDto> LineItems { get; set; } = new();
        public List<UpdatePODepositDto> Deposits { get; set; } = new();
        public List<UpdatePOShippingChargeDto> ShippingCharges { get; set; } = new();
        public List<UpdatePODiscountDto> Discounts { get; set; } = new();
        public List<UpdatePOAdjustmentDto> Adjustments { get; set; } = new();
        public List<IFormFile> NewAttachments { get; set; } = new();
        public List<Guid> AttachmentsToDelete { get; set; } = new();
        public List<string> NewNotes { get; set; } = new();

    }
}
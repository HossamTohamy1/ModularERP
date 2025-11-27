using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder
{
    public class CreatePurchaseOrderCommand : IRequest<ResponseViewModel<CreatePurchaseOrderDto>>
    {
        public Guid CompanyId { get; set; }
        public Guid SupplierId { get; set; }
        public string CurrencyCode { get; set; } = "SAR";
        public DateTime PODate { get; set; } = DateTime.UtcNow;
        public Guid? PaymentTermId { get; set; }
        public string? Notes { get; set; }
        public string? Terms { get; set; }

        public List<CreatePOLineItemDto> LineItems { get; set; } = new();
        public List<CreatePODepositDto> Deposits { get; set; } = new();
        public List<CreatePOShippingChargeDto> ShippingCharges { get; set; } = new();
        public List<CreatePODiscountDto> Discounts { get; set; } = new();
        public List<CreatePOAdjustmentDto> Adjustments { get; set; } = new();
        public List<IFormFile> Attachments { get; set; } = new();
        public List<string> PONotes { get; set; } = new();
    }

}

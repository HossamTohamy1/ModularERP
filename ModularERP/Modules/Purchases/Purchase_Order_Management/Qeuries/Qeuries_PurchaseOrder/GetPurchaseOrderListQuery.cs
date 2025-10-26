using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Warehouses.Validators;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_PurchaseOrder
{
    public class GetPurchaseOrderListQuery : IRequest<ResponseViewModel<DTO.DTO_PurchaseOrder.PagedResult<PurchaseOrderListDto>>>
    {
        public Guid? CompanyId { get; set; }
        public Guid? SupplierId { get; set; }
        public string? DocumentStatus { get; set; }
        public string? ReceptionStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

}

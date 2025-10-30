using AutoMapper;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.WorkFlow.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Mapping
{
    public class POStatusMappingProfile : Profile
    {
        public POStatusMappingProfile()
        {
            // PurchaseOrder -> POStatusDto
            CreateMap<PurchaseOrder, POStatusDto>()
                .ForMember(d => d.PurchaseOrderId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.Name ?? ""))
                .ForMember(d => d.ReceivedPercentage, o => o.MapFrom(s => CalculateReceivedPercentage(s)))
                .ForMember(d => d.PaidPercentage, o => o.MapFrom(s => CalculatePaidPercentage(s)));

            // POAuditLog -> POActivityLogDto
            CreateMap<POAuditLog, POActivityLogDto>()
                .ForMember(d => d.PerformedAt, o => o.MapFrom(s => s.CreatedAt));

            // PurchaseOrder -> POPrintDto
            CreateMap<PurchaseOrder, POPrintDto>()
                .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.Company.Name ?? ""))
                .ForMember(d => d.CompanyAddress, o => o.MapFrom(s => "")) // Company doesn't have address fields
                .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.Name ?? ""))
                .ForMember(d => d.SupplierAddress, o => o.MapFrom(s => s.Supplier.Address ?? ""))
                .ForMember(d => d.LineItems, o => o.MapFrom(s => s.LineItems));

            // POLineItem -> POLineItemPrintDto
            CreateMap<POLineItem, POLineItemPrintDto>()
                .ForMember(d => d.LineNumber, o => o.Ignore())
                .ForMember(d => d.ProductName, o => o.MapFrom(s =>
                    s.Product != null ? s.Product.Name : s.Service != null ? s.Service.Name : ""));
        }

        private static decimal CalculateReceivedPercentage(PurchaseOrder po)
        {
            if (po?.LineItems == null || !po.LineItems.Any()) return 0;

            var totalQty = po.LineItems.Sum(x => x.Quantity);
            var receivedQty = po.LineItems.Sum(x => x.ReceivedQuantity);

            return totalQty > 0 ? Math.Round((receivedQty / totalQty) * 100, 2) : 0;
        }

        private static decimal CalculatePaidPercentage(PurchaseOrder po)
        {
            if (po == null || po.TotalAmount <= 0) return 0;

            var paidAmount = po.TotalAmount - po.AmountDue;
            return Math.Round((paidAmount / po.TotalAmount) * 100, 2);
        }
    }
}
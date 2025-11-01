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
                .ForMember(d => d.TotalAmount, o => o.MapFrom(s => CalculateTotalAmount(s)))
                .ForMember(d => d.AmountDue, o => o.MapFrom(s => CalculateAmountDue(s)))
                .ForMember(d => d.ReceivedPercentage, o => o.MapFrom(s => CalculateReceivedPercentage(s)))
                .ForMember(d => d.PaidPercentage, o => o.MapFrom(s => CalculatePaidPercentage(s)))
                .ForMember(d => d.PODate, o => o.MapFrom(s => s.CreatedAt));

            // POAuditLog -> POActivityLogDto
            CreateMap<POAuditLog, POActivityLogDto>()
                .ForMember(d => d.PerformedAt, o => o.MapFrom(s => s.CreatedAt));

            // PurchaseOrder -> POPrintDto
            CreateMap<PurchaseOrder, POPrintDto>()
                .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.Company.Name ?? ""))
                .ForMember(d => d.CompanyAddress, o => o.MapFrom(s => ""))
                .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.Name ?? ""))
                .ForMember(d => d.SupplierAddress, o => o.MapFrom(s => s.Supplier.Address ?? ""))
                .ForMember(d => d.LineItems, o => o.MapFrom(s => s.LineItems));

            // POLineItem -> POLineItemPrintDto
            CreateMap<POLineItem, POLineItemPrintDto>()
                .ForMember(d => d.LineNumber, o => o.Ignore())
                .ForMember(d => d.ProductName, o => o.MapFrom(s =>
                    s.Product != null ? s.Product.Name : s.Service != null ? s.Service.Name : ""));
        }

        // ✅ Calculate Total Amount from Line Items
        private static decimal CalculateTotalAmount(PurchaseOrder po)
        {
            if (po?.LineItems == null || !po.LineItems.Any()) return 0;

            return po.LineItems
                .Where(li => !li.IsDeleted)
                .Sum(li => li.LineTotal);
        }

        // ✅ Calculate Amount Due (Total - Paid Deposits)
        private static decimal CalculateAmountDue(PurchaseOrder po)
        {
            if (po == null) return 0;

            var totalAmount = CalculateTotalAmount(po);
            var paidAmount = po.Deposits?
                .Where(d => d.AlreadyPaid && !d.IsDeleted)
                .Sum(d => d.Amount) ?? 0;

            return Math.Round(totalAmount - paidAmount, 4);
        }

        // ✅ Calculate Received Percentage
        private static decimal CalculateReceivedPercentage(PurchaseOrder po)
        {
            if (po?.LineItems == null || !po.LineItems.Any()) return 0;

            var totalQty = po.LineItems
                .Where(li => !li.IsDeleted)
                .Sum(x => x.Quantity);

            var receivedQty = po.LineItems
                .Where(li => !li.IsDeleted)
                .Sum(x => x.ReceivedQuantity);

            return totalQty > 0 ? Math.Round((receivedQty / totalQty) * 100, 2) : 0;
        }

        // ✅ Calculate Paid Percentage (Fixed Logic)
        private static decimal CalculatePaidPercentage(PurchaseOrder po)
        {
            if (po == null) return 0;

            var totalAmount = CalculateTotalAmount(po);
            if (totalAmount <= 0) return 0;

            var paidAmount = po.Deposits?
                .Where(d => d.AlreadyPaid && !d.IsDeleted)
                .Sum(d => d.Amount) ?? 0;

            // ✅ Cap at 100% (prevent showing 112%)
            var percentage = (paidAmount / totalAmount) * 100;
            return Math.Round(Math.Min(percentage, 100), 2);
        }
    }
}
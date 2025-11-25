using AutoMapper;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.KPIs.DTO;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;

namespace ModularERP.Modules.Purchases.KPIs.Mapping
{
    public class KPIMappingProfile : Profile
    {
        public KPIMappingProfile()
        {
            // Purchase Order Projections
            CreateMap<PurchaseOrder, MonthlyVolumeDto>()
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.PODate.Year))
                .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.PODate.Month))
                .ForMember(dest => dest.MonthName, opt => opt.MapFrom(src => src.PODate.ToString("MMMM yyyy")))
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.OrderCount, opt => opt.Ignore());

            // Supplier Payment Projections
            CreateMap<SupplierPayment, MonthlyPaymentDto>()
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.PaymentDate.Year))
                .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.PaymentDate.Month))
                .ForMember(dest => dest.MonthName, opt => opt.MapFrom(src => src.PaymentDate.ToString("MMMM yyyy")))
                .ForMember(dest => dest.TotalPaid, opt => opt.Ignore())
                .ForMember(dest => dest.TotalDue, opt => opt.Ignore());

            // Purchase Invoice Projections
            CreateMap<PurchaseInvoice, PaymentStatusBreakdownDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.PaymentStatus))
                .ForMember(dest => dest.Count, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.Percentage, opt => opt.Ignore());

            // Supplier Projections
            CreateMap<Supplier, TopSupplierDto>()
                .ForMember(dest => dest.SupplierId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TotalPurchaseAmount, opt => opt.Ignore())
                .ForMember(dest => dest.OrderCount, opt => opt.Ignore())
                .ForMember(dest => dest.OnTimeDeliveryRate, opt => opt.Ignore());
        }
    }
}
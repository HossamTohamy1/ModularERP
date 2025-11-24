using AutoMapper;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments;
using ModularERP.Modules.Purchases.Invoicing.Models;

namespace ModularERP.Modules.Purchases.Invoicing.Features.SupplierPayments.Mapping
{
    public class SupplierPaymentMappingProfile : Profile
    {
        public SupplierPaymentMappingProfile()
        {
            // Entity to DTO
            CreateMap<SupplierPayment, SupplierPaymentDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name))
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.InvoiceNumber : null))
                .ForMember(dest => dest.PONumber, opt => opt.MapFrom(src => src.PurchaseOrder != null ? src.PurchaseOrder.PONumber : null))
                .ForMember(dest => dest.VoidedByName, opt => opt.MapFrom(src => src.VoidedByUser != null ? src.VoidedByUser.UserName : null))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy));

            // Create DTO to Entity
            CreateMap<CreateSupplierPaymentDto, SupplierPayment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentNumber, opt => opt.Ignore()) // Auto-generated
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Draft"))
                .ForMember(dest => dest.UnallocatedAmount, opt => opt.MapFrom(src =>
                    src.AllocatedAmount.HasValue ? src.Amount - src.AllocatedAmount.Value : 0))
                .ForMember(dest => dest.AllocatedAmount, opt => opt.MapFrom(src =>
                    src.AllocatedAmount ?? src.Amount));

            // Update DTO to Entity
            CreateMap<UpdateSupplierPaymentDto, SupplierPayment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseOrderId, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentNumber, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentType, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());
        }
    }
}
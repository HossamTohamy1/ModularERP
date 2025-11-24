using AutoMapper;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication;
using ModularERP.Modules.Purchases.Invoicing.Models;

namespace ModularERP.Modules.Purchases.Invoicing.Mapping
{
    public class PaymentApplicationProfile : Profile
    {
        public PaymentApplicationProfile()
        {
            // PaymentAllocation -> PaymentAllocationResponseDto (with projections)
            CreateMap<PaymentAllocation, PaymentAllocationResponseDto>()
                .ForMember(dest => dest.PaymentNumber, opt => opt.MapFrom(src => src.Payment.PaymentNumber))
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice.InvoiceNumber));

            // ApplyPaymentDto -> PaymentAllocation (for creating allocations)
            CreateMap<InvoiceAllocationDto, PaymentAllocation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
                .ForMember(dest => dest.AllocationDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsVoided, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            // PayInvoiceDto -> SupplierPayment
            CreateMap<PayInvoiceDto, SupplierPayment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentNumber, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentType, opt => opt.MapFrom(src => "AgainstInvoice"))
                .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
                .ForMember(dest => dest.AllocatedAmount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.UnallocatedAmount, opt => opt.MapFrom(src => 0m))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Posted"))
                .ForMember(dest => dest.IsVoid, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            // QuickPaySupplierDto -> SupplierPayment
            CreateMap<QuickPaySupplierDto, SupplierPayment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentNumber, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseOrderId, opt => opt.Ignore())
                .ForMember(dest => dest.AllocatedAmount, opt => opt.MapFrom(src =>
                    src.InvoiceAllocations != null && src.InvoiceAllocations.Any()
                        ? src.InvoiceAllocations.Sum(a => a.AllocatedAmount)
                        : 0m))
                .ForMember(dest => dest.UnallocatedAmount, opt => opt.MapFrom(src =>
                    src.InvoiceAllocations != null && src.InvoiceAllocations.Any()
                        ? src.Amount - src.InvoiceAllocations.Sum(a => a.AllocatedAmount)
                        : src.Amount))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Posted"))
                .ForMember(dest => dest.IsVoid, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            // SupplierPayment -> PaymentApplicationSummaryDto
            // SupplierPayment -> PaymentApplicationSummaryDto
            CreateMap<SupplierPayment, PaymentApplicationSummaryDto>()
                .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.Id))  // ✅ إضافة
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.AllocationsCount, opt => opt.MapFrom(src => src.Allocations.Count(a => !a.IsVoided)))
                .ForMember(dest => dest.Allocations, opt => opt.Ignore());  // ✅ هنملاها يدوي
                                                                            }
        }
}

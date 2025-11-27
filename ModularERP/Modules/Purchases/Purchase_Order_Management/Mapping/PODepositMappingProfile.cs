using AutoMapper;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PODeposite;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PODeposite;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Mapping
{
    public class PODepositMappingProfile : Profile
    {
        public PODepositMappingProfile()
        {
            // ========================================
            // PODeposit → PODepositResponseDto
            // ========================================
            CreateMap<PODeposit, PODepositResponseDto>()
                .ForMember(dest => dest.PaymentMethodName,
                    opt => opt.MapFrom(src => src.PaymentMethod.Name))
                .ForMember(dest => dest.PaymentMethodCode,
                    opt => opt.MapFrom(src => src.PaymentMethod.Code))
                .ForMember(dest => dest.POTotal,
                    opt => opt.Ignore())
                .ForMember(dest => dest.TotalDeposits,
                    opt => opt.Ignore())
                .ForMember(dest => dest.RemainingBalance,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PaymentStatus,
                    opt => opt.Ignore());

            // ========================================
            // PODeposit → PODepositDto (for listings)
            // ========================================
            CreateMap<PODeposit, PODepositDto>()
                .ForMember(dest => dest.PaymentMethodName,
                    opt => opt.MapFrom(src => src.PaymentMethod.Name))
                .ForMember(dest => dest.PaymentMethodCode,
                    opt => opt.MapFrom(src => src.PaymentMethod.Code))
                .ForMember(dest => dest.PaymentMethodRequiresReference,
                    opt => opt.MapFrom(src => src.PaymentMethod.RequiresReference));

            // ========================================
            // CreatePODepositCommand → PODeposit
            // ========================================
            CreateMap<CreatePODepositCommand, PODeposit>()
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsActive,
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseOrder,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethod,
                    opt => opt.Ignore());

            // ========================================
            // UpdatePODepositCommand → PODeposit
            // ========================================
            CreateMap<UpdatePODepositCommand, PODeposit>()
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore())
.ForMember(dest => dest.UpdatedAt,
    opt => opt.MapFrom(src => DateTime.UtcNow))

                .ForMember(dest => dest.IsActive,
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseOrder,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethod,
                    opt => opt.Ignore());

            // ========================================
            // CreatePODepositDto → PODeposit
            // ========================================
            CreateMap<CreatePODepositDto, PODeposit>()
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseOrderId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsActive,
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseOrder,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethod,
                    opt => opt.Ignore());

            // ========================================
            // UpdatePODepositDto → PODeposit
            // ========================================
            CreateMap<UpdatePODepositDto, PODeposit>()
                .ForMember(dest => dest.PurchaseOrderId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore())
.ForMember(dest => dest.UpdatedAt,
    opt => opt.MapFrom(src => DateTime.UtcNow))

                .ForMember(dest => dest.IsActive,
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseOrder,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethod,
                    opt => opt.Ignore());
        }
    }
}
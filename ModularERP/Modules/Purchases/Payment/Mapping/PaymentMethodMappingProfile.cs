using AutoMapper;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentMethod;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentMethod;

namespace ModularERP.Modules.Purchases.Payment.Mapping
{
    public class PaymentMethodMappingProfile : Profile
    {
        public PaymentMethodMappingProfile()
        {
            // Entity to DTO
            CreateMap<PaymentMethod, PaymentMethodDto>();

            // ✅ Command to Entity - للـ Create
            CreateMap<CreatePaymentMethodCommand, PaymentMethod>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierPayments, opt => opt.Ignore())
                .ForMember(dest => dest.PODeposits, opt => opt.Ignore());

            // ✅ Command to Entity - للـ Update
            CreateMap<UpdatePaymentMethodCommand, PaymentMethod>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierPayments, opt => opt.Ignore())
                .ForMember(dest => dest.PODeposits, opt => opt.Ignore());

            // Create DTO to Entity (إذا كنت تستخدمه)
            CreateMap<CreatePaymentMethodDto, PaymentMethod>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierPayments, opt => opt.Ignore())
                .ForMember(dest => dest.PODeposits, opt => opt.Ignore());

            // Update DTO to Entity (إذا كنت تستخدمه)
            CreateMap<UpdatePaymentMethodDto, PaymentMethod>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierPayments, opt => opt.Ignore())
                .ForMember(dest => dest.PODeposits, opt => opt.Ignore());
        }
    }
}
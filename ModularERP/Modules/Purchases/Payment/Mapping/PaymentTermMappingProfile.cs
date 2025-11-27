using AutoMapper;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentTerm;
using ModularERP.Modules.Purchases.Payment.Models;

namespace ModularERP.Modules.Purchases.Payment.Mapping
{
    public class PaymentTermMappingProfile : Profile
    {
        public PaymentTermMappingProfile()
        {
            // Entity to DTO
            CreateMap<PaymentTerm, PaymentTermResponseDto>();

            // DTO to Entity
            CreateMap<CreatePaymentTermDto, PaymentTerm>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore());

            CreateMap<UpdatePaymentTermDto, PaymentTerm>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

using AutoMapper;
using ModularERP.Modules.Finance.Features.Taxs.DTO;
using ModularERP.Modules.Finance.Features.Taxs.Models;

namespace ModularERP.Modules.Finance.Features.Taxs.Mapping
{
    public class TaxProfile : Profile
    {
        public TaxProfile()
        {
            CreateMap<CreateTaxDto, Tax>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.VoucherTaxes, opt => opt.Ignore());

            CreateMap<UpdateTaxDto, Tax>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.VoucherTaxes, opt => opt.Ignore());

            CreateMap<Tax, TaxResponseDto>()
                .ForMember(dest => dest.VoucherTaxesCount, opt => opt.MapFrom(src => src.VoucherTaxes.Count));

            CreateMap<Tax, TaxListDto>()
                .ForMember(dest => dest.CanDelete, opt => opt.MapFrom(src => !src.VoucherTaxes.Any()));
        }
    }
}

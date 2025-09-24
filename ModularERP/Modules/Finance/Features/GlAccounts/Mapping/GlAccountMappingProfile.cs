using AutoMapper;
using ModularERP.Modules.Finance.Features.GlAccounts.DTO;
using ModularERP.Modules.Finance.Features.GlAccounts.Models;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Mapping
{
    public class GlAccountMappingProfile : Profile
    {
        public GlAccountMappingProfile()
        {
            CreateMap<CreateGlAccountDto, GlAccount>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryVouchers, opt => opt.Ignore())
                .ForMember(dest => dest.JournalVouchers, opt => opt.Ignore())
                .ForMember(dest => dest.LedgerEntries, opt => opt.Ignore());

            CreateMap<UpdateGlAccountDto, GlAccount>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryVouchers, opt => opt.Ignore())
                .ForMember(dest => dest.JournalVouchers, opt => opt.Ignore())
                .ForMember(dest => dest.LedgerEntries, opt => opt.Ignore());

            CreateMap<GlAccount, GlAccountResponseDto>()
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty));
        }
    }
}

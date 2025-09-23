using AutoMapper;
using ModularERP.Modules.Finance.Features.Companys.DTO;
using ModularERP.Modules.Finance.Features.Companys.Models;

namespace ModularERP.Modules.Finance.Features.Companys.Mapping
{
    public class CompanyMappingProfile : Profile
    {
        public CompanyMappingProfile()
        {
            CreateMap<CreateCompanyDto, Company>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Treasuries, opt => opt.Ignore())
                .ForMember(dest => dest.BankAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.GlAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.Vouchers, opt => opt.Ignore());

            CreateMap<UpdateCompanyDto, Company>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Treasuries, opt => opt.Ignore())
                .ForMember(dest => dest.BankAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.GlAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.Vouchers, opt => opt.Ignore());

            CreateMap<Company, CompanyResponseDto>();
        }
    }
}

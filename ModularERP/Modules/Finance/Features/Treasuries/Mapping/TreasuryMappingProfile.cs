using AutoMapper;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;

namespace ModularERP.Modules.Finance.Features.Treasuries.Mapping
{
    public class TreasuryMappingProfile : Profile
    {
        public TreasuryMappingProfile()
        {
            // DTO to Entity mappings
            CreateMap<CreateTreasuryDto, Treasury>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.JournalAccount, opt => opt.Ignore())
                .ForMember(dest => dest.Vouchers, opt => opt.Ignore());

            CreateMap<UpdateTreasuryDto, Treasury>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.JournalAccount, opt => opt.Ignore())
                .ForMember(dest => dest.Vouchers, opt => opt.Ignore());

            // Entity to DTO mappings
            CreateMap<Treasury, TreasuryDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.Ignore())
                .ForMember(dest => dest.CurrencyName, opt => opt.Ignore())
                .ForMember(dest => dest.JournalAccountName, opt => opt.Ignore())
                .ForMember(dest => dest.VouchersCount, opt => opt.Ignore());

            CreateMap<Treasury, TreasuryListDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.Ignore())
                .ForMember(dest => dest.CurrencyName, opt => opt.Ignore())
                .ForMember(dest => dest.JournalAccountName, opt => opt.Ignore())
                .ForMember(dest => dest.VouchersCount, opt => opt.Ignore());

            CreateMap<Treasury, TreasuryCreatedDto>();

            // Command to DTO mappings (for flexibility)
            CreateMap<CreateTreasuryCommand, CreateTreasuryDto>()
                .ConstructUsing(src => src.Treasury);

            CreateMap<UpdateTreasuryCommand, UpdateTreasuryDto>()
                .ConstructUsing(src => src.Treasury);
        }
    }
}

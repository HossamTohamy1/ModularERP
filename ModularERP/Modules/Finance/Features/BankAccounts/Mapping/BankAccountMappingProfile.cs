using AutoMapper;
using ModularERP.Modules.Finance.Features.BankAccounts.Commands;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Mapping
{
    public class BankAccountMappingProfile : Profile
    {
        public BankAccountMappingProfile()
        {
            // DTO to Entity mappings
            CreateMap<CreateBankAccountDto, BankAccount>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.JournalAccount, opt => opt.Ignore())  // Add this line
                .ForMember(dest => dest.Vouchers, opt => opt.Ignore());

            CreateMap<UpdateBankAccountDto, BankAccount>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.JournalAccount, opt => opt.Ignore())  // Add this line
                .ForMember(dest => dest.Vouchers, opt => opt.Ignore());

            // Entity to DTO mappings
            CreateMap<BankAccount, BankAccountDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.Ignore())
                .ForMember(dest => dest.CurrencyName, opt => opt.Ignore())
                .ForMember(dest => dest.JournalAccountName, opt => opt.Ignore())  // Add this line
                .ForMember(dest => dest.VouchersCount, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            CreateMap<BankAccount, BankAccountListDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.Ignore())
                .ForMember(dest => dest.CurrencyName, opt => opt.Ignore())
                .ForMember(dest => dest.JournalAccountName, opt => opt.Ignore())  // Add this line
                .ForMember(dest => dest.VouchersCount, opt => opt.Ignore());

            CreateMap<BankAccount, BankAccountCreatedDto>();

            // Command to DTO mappings (for flexibility)
            CreateMap<CreateBankAccountCommand, CreateBankAccountDto>()
                .ConstructUsing(src => src.BankAccount);

            CreateMap<UpdateBankAccountCommand, UpdateBankAccountDto>()
                .ConstructUsing(src => src.BankAccount);
        }
    }
}
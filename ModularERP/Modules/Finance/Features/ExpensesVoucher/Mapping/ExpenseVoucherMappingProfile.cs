using AutoMapper;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Mapping
{
    public class ExpenseVoucherMappingProfile : Profile
    {
        public ExpenseVoucherMappingProfile()
        {
            CreateMap<CreateExpenseVoucherDto, Voucher>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Expense"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Draft"))
                .ForMember(dest => dest.CategoryAccountId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.WalletType, opt => opt.MapFrom(src => src.Source.Type))
                .ForMember(dest => dest.WalletId, opt => opt.MapFrom(src => src.Source.Id))
                .ForMember(dest => dest.CounterpartyType, opt => opt.MapFrom(src => src.Counterparty != null ? src.Counterparty.Type : null))
                .ForMember(dest => dest.CounterpartyId, opt => opt.MapFrom(src => src.Counterparty != null ? src.Counterparty.Id : null))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.VoucherTaxes, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());

            CreateMap<TaxLineDto, VoucherTax>()
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => "Expense"));

            CreateMap<Voucher, ExpenseVoucherResponseDto>();

            CreateMap<VoucherTax, TaxLineResponseDto>();

            CreateMap<VoucherAttachment, AttachmentResponseDto>();
        }
    }
}

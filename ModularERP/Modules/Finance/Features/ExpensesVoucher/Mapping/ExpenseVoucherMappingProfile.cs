using AutoMapper;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO.ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Mapping
{
    public class ExpenseVoucherMappingProfile : Profile
    {
        public ExpenseVoucherMappingProfile()
        {
            // ✅ CreateExpenseVoucherDto to Voucher mapping
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

            // ✅ TaxLineDto to VoucherTax mapping
            CreateMap<TaxLineDto, VoucherTax>()
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => "Expense"));

            // ✅ Voucher to ExpenseVoucherResponseDto mapping (with Source & Counterparty)
            CreateMap<Voucher, ExpenseVoucherResponseDto>()
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src =>
                    new WalletDto
                    {
                        Type = src.WalletType != null ? src.WalletType.ToString() : "",
                        Id = src.WalletId != null && src.WalletId != Guid.Empty ? src.WalletId : Guid.Empty
                    }))
                .ForMember(dest => dest.Counterparty, opt => opt.MapFrom(src =>
                    (src.CounterpartyId != null && src.CounterpartyId != Guid.Empty && src.CounterpartyType != null)
                        ? new CounterpartyDto
                        {
                            Type = src.CounterpartyType.ToString(),
                            Id = src.CounterpartyId
                        }
                        : null))
                .ForMember(dest => dest.TaxLines, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());

            // ✅ CreateExpenseVoucherDto to ExpenseVoucherResponseDto mapping (for create response)
            CreateMap<CreateExpenseVoucherDto, ExpenseVoucherResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
                .ForMember(dest => dest.Counterparty, opt => opt.MapFrom(src => src.Counterparty))
                .ForMember(dest => dest.TaxLines, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());

            // ✅ VoucherTax to TaxLineResponseDto mapping
            // ✅ VoucherTax to TaxLineResponseDto mapping
            CreateMap<VoucherTax, TaxLineResponseDto>()
                .ForMember(dest => dest.TaxProfileName, opt => opt.MapFrom(src => src.TaxProfile.Name))
                .ForMember(dest => dest.TaxComponentName, opt => opt.MapFrom(src => src.TaxComponent.Name));

            CreateMap<VoucherAttachment, AttachmentResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.Filename))
                .ForMember(dest => dest.FileUrl, opt => opt.MapFrom(src => src.FilePath))
                .ForMember(dest => dest.MimeType, opt => opt.MapFrom(src => src.MimeType ?? string.Empty))
                .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.Checksum, opt => opt.MapFrom(src => src.Checksum))
                .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => src.UploadedAt))
                .ForMember(dest => dest.UploadedBy, opt => opt.MapFrom(src => src.UploadedBy));

            CreateMap<UpdateExpenseVoucherDto, Voucher>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Expense"))
                .ForMember(dest => dest.CategoryAccountId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.WalletType, opt => opt.MapFrom(src => src.Source.Type))
                .ForMember(dest => dest.WalletId, opt => opt.MapFrom(src => src.Source.Id))
                .ForMember(dest => dest.CounterpartyType, opt => opt.MapFrom(src => src.Counterparty != null ? src.Counterparty.Type : null))
                .ForMember(dest => dest.CounterpartyId, opt => opt.MapFrom(src => src.Counterparty != null ? src.Counterparty.Id : null))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.VoucherTaxes, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());
        }
    }
}
using AutoMapper;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_DebitNote;
using ModularERP.Modules.Purchases.Refunds.Models;

namespace ModularERP.Modules.Purchases.Refunds.Mapping
{
    public class DebitNoteMappingProfile : Profile
    {
        public DebitNoteMappingProfile()
        {
            // DebitNote -> DebitNoteListDto
            CreateMap<DebitNote, DebitNoteListDto>()
                .ForMember(dest => dest.RefundNumber,
                    opt => opt.MapFrom(src => src.Refund.RefundNumber))
                .ForMember(dest => dest.SupplierName,
                    opt => opt.MapFrom(src => src.Supplier.Name));

            // DebitNote -> DebitNoteDetailsDto
            CreateMap<DebitNote, DebitNoteDetailsDto>()
                .ForMember(dest => dest.RefundNumber,
                    opt => opt.MapFrom(src => src.Refund.RefundNumber))
                .ForMember(dest => dest.SupplierName,
                    opt => opt.MapFrom(src => src.Supplier.Name))
                .ForMember(dest => dest.SupplierEmail,
                    opt => opt.MapFrom(src => src.Supplier.Email))
                .ForMember(dest => dest.SupplierPhone,
                    opt => opt.MapFrom(src => src.Supplier.Phone))
                .ForMember(dest => dest.Refund,
                    opt => opt.MapFrom(src => src.Refund));

            // PurchaseRefund -> RefundSummaryDto
            CreateMap<PurchaseRefund, RefundSummaryDto>()
                .ForMember(dest => dest.PONumber,
                    opt => opt.MapFrom(src => src.PurchaseOrder.PONumber));

            // DebitNote -> SupplierDebitNoteDto
            CreateMap<DebitNote, SupplierDebitNoteDto>()
                .ForMember(dest => dest.RefundNumber,
                    opt => opt.MapFrom(src => src.Refund.RefundNumber));
        }
    }
}

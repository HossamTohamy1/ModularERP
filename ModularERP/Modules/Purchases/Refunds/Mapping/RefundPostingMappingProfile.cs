using AutoMapper;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Models;

namespace ModularERP.Modules.Purchases.Refunds.Mapping
{
    public class RefundPostingMappingProfile : Profile
    {
        public RefundPostingMappingProfile()
        {
            // DebitNote -> DebitNoteDto
            CreateMap<DebitNote, DebitNoteDto>()
                .ForMember(dest => dest.RefundNumber, opt => opt.MapFrom(src => src.Refund.RefundNumber))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name));
        }
    }
}

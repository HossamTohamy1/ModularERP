using AutoMapper;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POAttachment;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Mapping
{
    public class POAttachmentMappingProfile : Profile
    {
        public POAttachmentMappingProfile()
        {
            CreateMap<POAttachment, POAttachmentResponseDto>()
                .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src =>
                    src.UploadedByUser != null
                        ? $"{src.UploadedByUser.FirstName} {src.UploadedByUser.LastName}".Trim()
                        : "Unknown"));
        }
    }
}
using AutoMapper;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListAssignment;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Mapping
{
    public class PriceListAssignmentMappingProfile : Profile
    {
        public PriceListAssignmentMappingProfile()
        {
            // Entity to DTO
            CreateMap<PriceListAssignment, PriceListAssignmentDto>()
                .ForMember(dest => dest.PriceListName,
                    opt => opt.MapFrom(src => src.PriceList.Name));

            // Create DTO to Entity
            CreateMap<CreatePriceListAssignmentDto, PriceListAssignment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.PriceList, opt => opt.Ignore());

            // Update DTO to Entity
            CreateMap<UpdatePriceListAssignmentDto, PriceListAssignment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.PriceList, opt => opt.Ignore());
        }
    }
}
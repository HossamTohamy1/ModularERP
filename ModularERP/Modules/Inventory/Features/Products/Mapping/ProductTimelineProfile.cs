using AutoMapper;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductTimeline;
using ModularERP.Modules.Inventory.Features.Products.Models;

namespace ModularERP.Modules.Inventory.Features.Products.Mapping
{
    public class ProductTimelineProfile : Profile
    {
        public ProductTimelineProfile()
        {
            CreateMap<ProductTimeline, ProductTimelineDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.ProductId, opt => opt.MapFrom(s => s.ProductId))
                .ForMember(d => d.ActionType, opt => opt.MapFrom(s => s.ActionType))
                .ForMember(d => d.ItemReference, opt => opt.MapFrom(s => s.ItemReference))
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.UserId))
                .ForMember(d => d.StockBalance, opt => opt.MapFrom(s => s.StockBalance))
                .ForMember(d => d.AveragePrice, opt => opt.MapFrom(s => s.AveragePrice))
                .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
                .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt));
        }
    }
}

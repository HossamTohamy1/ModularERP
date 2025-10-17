using AutoMapper;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductTimeline;
using ModularERP.Modules.Inventory.Features.Products.Models;

namespace ModularERP.Modules.Inventory.Features.Products.Mapping
{
    public class ActivityLogProfile : Profile
    {
        public ActivityLogProfile()
        {
            CreateMap<ActivityLog, ActivityLogDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.ProductId, opt => opt.MapFrom(s => s.ProductId))
                .ForMember(d => d.ActionType, opt => opt.MapFrom(s => s.ActionType))
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.UserId))
                .ForMember(d => d.BeforeValues, opt => opt.MapFrom(s => s.BeforeValues))
                .ForMember(d => d.AfterValues, opt => opt.MapFrom(s => s.AfterValues))
                .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
                .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt));
        }
    }

}

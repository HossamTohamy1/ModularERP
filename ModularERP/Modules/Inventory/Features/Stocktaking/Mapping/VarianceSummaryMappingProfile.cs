using AutoMapper;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Mapping
{
    public class VarianceSummaryMappingProfile : Profile
    {
        public VarianceSummaryMappingProfile()
        {
            CreateMap<StocktakingHeader, VarianceSummaryDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.TotalRecordedItems, opt => opt.MapFrom(src => src.Lines.Count))
                .ForMember(dest => dest.TotalShortages, opt => opt.MapFrom(src =>
                    src.Lines.Count(l => (l.VarianceQty ?? 0) < 0)))
                .ForMember(dest => dest.TotalOverages, opt => opt.MapFrom(src =>
                    src.Lines.Count(l => (l.VarianceQty ?? 0) > 0)))
                .ForMember(dest => dest.TotalShortageQty, opt => opt.MapFrom(src =>
                    src.Lines.Where(l => (l.VarianceQty ?? 0) < 0).Sum(l => Math.Abs(l.VarianceQty ?? 0))))
                .ForMember(dest => dest.TotalOverageQty, opt => opt.MapFrom(src =>
                    src.Lines.Where(l => (l.VarianceQty ?? 0) > 0).Sum(l => l.VarianceQty ?? 0)))
                .ForMember(dest => dest.TotalShortageValue, opt => opt.MapFrom(src =>
                    src.Lines.Where(l => (l.VarianceQty ?? 0) < 0).Sum(l => Math.Abs(l.VarianceValue ?? 0))))
                .ForMember(dest => dest.TotalOverageValue, opt => opt.MapFrom(src =>
                    src.Lines.Where(l => (l.VarianceQty ?? 0) > 0).Sum(l => (l.VarianceValue ?? 0))))
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src =>
                    src.ApprovedByUser != null ? src.ApprovedByUser.UserName : "System"));
        }
    }
}

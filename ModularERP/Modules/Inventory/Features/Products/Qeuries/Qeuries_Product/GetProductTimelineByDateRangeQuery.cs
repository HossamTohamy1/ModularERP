using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ProductTimeline;

namespace ModularERP.Modules.Inventory.Features.Products.Qeuries.Qeuries_Product
{
    public class GetProductTimelineByDateRangeQuery : IRequest<ResponseViewModel<IEnumerable<ProductTimelineDto>>>
    {
        public Guid ProductId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

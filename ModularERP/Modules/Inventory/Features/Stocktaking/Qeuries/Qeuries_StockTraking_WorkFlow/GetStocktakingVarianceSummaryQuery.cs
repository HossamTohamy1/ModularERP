using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTraking_WorkFlow
{
    public class GetStocktakingVarianceSummaryQuery : IRequest<ResponseViewModel<VarianceSummaryDto>>
    {
        public Guid StocktakingId { get; set; }
        public Guid CompanyId { get; set; }
    }
}

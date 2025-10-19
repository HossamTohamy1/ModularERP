using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow
{
    public class ReviewStocktakingCommand : IRequest<ResponseViewModel<ReviewStocktakingDto>>
    {
        public Guid StocktakingId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
    }
}

using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow
{
    public class PostStocktakingCommand : IRequest<ResponseViewModel<PostStocktakingDto>>
    {
        public Guid StocktakingId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public bool ForcePost { get; set; } = false;
    }
}

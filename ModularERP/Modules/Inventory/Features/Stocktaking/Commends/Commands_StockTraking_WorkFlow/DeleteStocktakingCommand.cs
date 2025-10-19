using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow
{
    public class DeleteStocktakingCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid StocktakingId { get; set; }
        public Guid CompanyId { get; set; }
    }
}

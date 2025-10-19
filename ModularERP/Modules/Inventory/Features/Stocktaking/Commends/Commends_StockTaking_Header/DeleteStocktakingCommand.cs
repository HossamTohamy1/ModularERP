using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Header
{
    public record DeleteStocktakingCommand(Guid Id) : IRequest<ResponseViewModel<bool>>;

}

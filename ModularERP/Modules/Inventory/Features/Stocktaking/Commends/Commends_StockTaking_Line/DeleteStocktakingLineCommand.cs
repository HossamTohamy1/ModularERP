using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Line
{
    public class DeleteStocktakingLineCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid StocktakingId { get; set; }
        public Guid LineId { get; set; }
    }

}

using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockSnapshot
{
    public class DeleteSnapshotCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid StocktakingId { get; set; }
    }
}

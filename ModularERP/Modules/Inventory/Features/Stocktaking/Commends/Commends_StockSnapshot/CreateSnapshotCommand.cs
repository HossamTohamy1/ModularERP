using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockSnapshot
{
    public class CreateSnapshotCommand : IRequest<ResponseViewModel<SnapshotListDto>>
    {
        public Guid StocktakingId { get; set; }
    }

}

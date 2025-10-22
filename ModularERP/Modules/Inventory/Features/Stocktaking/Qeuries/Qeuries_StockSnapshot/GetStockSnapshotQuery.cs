using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockSnapshot
{
    public class GetStockSnapshotQuery : IRequest<ResponseViewModel<SnapshotListDto>>
    {
        public Guid StocktakingId { get; set; }
    }
}


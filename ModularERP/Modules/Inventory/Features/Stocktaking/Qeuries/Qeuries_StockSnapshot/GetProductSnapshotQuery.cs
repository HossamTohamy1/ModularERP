using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockSnapshot
{
    public class GetProductSnapshotQuery : IRequest<ResponseViewModel<StockSnapshotDto>>
    {
        public Guid StocktakingId { get; set; }
        public Guid ProductId { get; set; }
    }
}

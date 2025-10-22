using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockSnapshot
{
    public class CompareSnapshotQuery : IRequest<ResponseViewModel<List<SnapshotComparisonDto>>>
    {
        public Guid StocktakingId { get; set; }
        public decimal DriftThreshold { get; set; } = 5; // 5% default threshold
    }
}

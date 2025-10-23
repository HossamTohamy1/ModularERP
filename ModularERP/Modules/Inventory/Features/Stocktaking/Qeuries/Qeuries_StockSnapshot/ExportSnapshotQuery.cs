using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockSnapshot
{
    public class ExportSnapshotQuery : IRequest<ResponseViewModel<byte[]>>
    {
        public Guid StocktakingId { get; set; }
        public string Format { get; set; } = "csv";
    }
}

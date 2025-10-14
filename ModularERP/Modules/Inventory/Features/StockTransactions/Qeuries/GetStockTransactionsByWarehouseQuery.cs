using MediatR;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Qeuries
{
    public class GetStockTransactionsByWarehouseQuery : IRequest<List<StockTransactionDto>>
    {
        public Guid WarehouseId { get; set; }
        public Guid? ProductId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public GetStockTransactionsByWarehouseQuery(Guid warehouseId)
        {
            WarehouseId = warehouseId;
        }
    }
}
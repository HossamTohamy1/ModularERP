using MediatR;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries
{
    public class GetStockTransactionsByProductQuery : IRequest<List<StockTransactionDto>>
    {
        public Guid ProductId { get; set; }
        public Guid? WarehouseId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public GetStockTransactionsByProductQuery(Guid productId)
        {
            ProductId = productId;
        }
    }
}

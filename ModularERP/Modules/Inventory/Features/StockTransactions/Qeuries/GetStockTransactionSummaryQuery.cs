using MediatR;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Qeuries
{
    public class GetStockTransactionSummaryQuery : IRequest<List<StockTransactionSummaryDto>>
    {
        public Guid? CompanyId { get; set; }
        public Guid? WarehouseId { get; set; }
        public Guid? ProductId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
using MediatR;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Qeuries
{
    public class GetAllStockTransactionsQuery : IRequest<List<StockTransactionDto>>
    {
        public Guid? CompanyId { get; set; }
        public Guid? WarehouseId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

}

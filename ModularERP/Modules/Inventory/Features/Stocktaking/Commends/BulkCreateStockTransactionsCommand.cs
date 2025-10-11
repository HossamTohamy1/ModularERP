using MediatR;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends
{
    public class BulkCreateStockTransactionsCommand : IRequest<List<StockTransactionDto>>
    {
        public List<CreateStockTransactionDto> Transactions { get; set; }

        public BulkCreateStockTransactionsCommand(List<CreateStockTransactionDto> transactions)
        {
            Transactions = transactions;
        }
    }
}
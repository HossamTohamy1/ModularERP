using MediatR;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Commends
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
using MediatR;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Commends
{
    public class CreateStockTransactionCommand : IRequest<StockTransactionDto>
    {
        public CreateStockTransactionDto Data { get; set; }

        public CreateStockTransactionCommand(CreateStockTransactionDto data)
        {
            Data = data;
        }
    }
}

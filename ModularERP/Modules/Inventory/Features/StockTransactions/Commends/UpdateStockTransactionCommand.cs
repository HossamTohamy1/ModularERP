using MediatR;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Commends
{
    public class UpdateStockTransactionCommand : IRequest<StockTransactionDto>
    {
        public UpdateStockTransactionDto Data { get; set; }

        public UpdateStockTransactionCommand(UpdateStockTransactionDto data)
        {
            Data = data;
        }
    }
}

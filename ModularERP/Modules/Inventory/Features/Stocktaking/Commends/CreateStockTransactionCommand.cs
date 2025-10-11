using MediatR;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends
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

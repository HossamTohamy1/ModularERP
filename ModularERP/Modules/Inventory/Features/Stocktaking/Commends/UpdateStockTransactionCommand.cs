using MediatR;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends
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

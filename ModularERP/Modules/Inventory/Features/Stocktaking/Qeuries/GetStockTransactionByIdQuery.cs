using MediatR;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries
{
    public class GetStockTransactionByIdQuery : IRequest<StockTransactionDto>
    {
        public Guid Id { get; set; }

        public GetStockTransactionByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}

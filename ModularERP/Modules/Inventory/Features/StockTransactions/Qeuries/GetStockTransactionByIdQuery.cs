using MediatR;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Qeuries
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

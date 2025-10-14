using MediatR;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Commends
{
    public class DeleteStockTransactionCommand : IRequest<bool>
    {
        public Guid Id { get; set; }

        public DeleteStockTransactionCommand(Guid id)
        {
            Id = id;
        }
    }
}

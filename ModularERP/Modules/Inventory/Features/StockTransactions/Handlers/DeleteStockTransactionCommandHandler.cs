using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.StockTransactions.Commends;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Handlers
{
    public class DeleteStockTransactionCommandHandler : IRequestHandler<DeleteStockTransactionCommand, bool>
    {
        private readonly IGeneralRepository<StockTransaction> _repository;

        public DeleteStockTransactionCommandHandler(IGeneralRepository<StockTransaction> repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteStockTransactionCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _repository.GetByID(request.Id);

            if (transaction == null)
                throw new NotFoundException("Stock transaction not found", FinanceErrorCode.NotFound);

            // Check if there are subsequent transactions
            var hasSubsequentTransactions = await _repository.GetAll()
                .AnyAsync(t => t.ProductId == transaction.ProductId &&
                              t.WarehouseId == transaction.WarehouseId &&
                              t.CreatedAt > transaction.CreatedAt,
                              cancellationToken);

            if (hasSubsequentTransactions)
            {
                throw new BusinessLogicException(
                    "Cannot delete transaction with subsequent transactions. Please delete or adjust later transactions first.",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            await _repository.Delete(request.Id);
            return true;
        }
    }

}

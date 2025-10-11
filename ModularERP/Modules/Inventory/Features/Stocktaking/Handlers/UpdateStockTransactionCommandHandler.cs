using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers
{
    public class UpdateStockTransactionCommandHandler : IRequestHandler<UpdateStockTransactionCommand, StockTransactionDto>
    {
        private readonly IGeneralRepository<StockTransaction> _transactionRepository;
        private readonly IMapper _mapper;

        public UpdateStockTransactionCommandHandler(
            IGeneralRepository<StockTransaction> transactionRepository,
            IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        public async Task<StockTransactionDto> Handle(UpdateStockTransactionCommand request, CancellationToken cancellationToken)
        {
            var existingTransaction = await _transactionRepository.GetByIDWithTracking(request.Data.Id);

            if (existingTransaction == null)
                throw new NotFoundException("Stock transaction not found", FinanceErrorCode.NotFound);

            // Get stock level before this transaction
            var previousStockLevel = await _transactionRepository.GetAll()
                .Where(t => t.ProductId == existingTransaction.ProductId &&
                           t.WarehouseId == existingTransaction.WarehouseId &&
                           t.CreatedAt < existingTransaction.CreatedAt)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => t.StockLevelAfter)
                .FirstOrDefaultAsync(cancellationToken);

            // Update allowed fields
            existingTransaction.TransactionType = request.Data.TransactionType;
            existingTransaction.Quantity = request.Data.Quantity;
            existingTransaction.UnitCost = request.Data.UnitCost;
            existingTransaction.ReferenceType = request.Data.ReferenceType;
            existingTransaction.ReferenceId = request.Data.ReferenceId;
            existingTransaction.UpdatedAt = DateTime.UtcNow;

            // Recalculate stock level after
            var quantityChange = request.Data.Quantity;
            if (request.Data.TransactionType == StockTransactionType.Sale ||
                request.Data.TransactionType == StockTransactionType.Transfer)
            {
                quantityChange = -quantityChange;
            }

            existingTransaction.StockLevelAfter = previousStockLevel + quantityChange;

            // Validate no negative stock
            if (existingTransaction.StockLevelAfter < 0)
            {
                throw new BusinessLogicException(
                    "Transaction update would result in negative stock",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            await _transactionRepository.SaveChanges();

            // Return updated transaction with projection
            var result = await _transactionRepository.GetAll()
                .Where(t => t.Id == existingTransaction.Id)
                .ProjectTo<StockTransactionDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return result!;
        }
    }
}

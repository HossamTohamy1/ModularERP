using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.StockTransactions.Commends;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Handlers
{
    public class CreateStockTransactionCommandHandler : IRequestHandler<CreateStockTransactionCommand, StockTransactionDto>
    {
        private readonly IGeneralRepository<StockTransaction> _transactionRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<Warehouse> _warehouseRepository;
        private readonly IMapper _mapper;

        public CreateStockTransactionCommandHandler(
            IGeneralRepository<StockTransaction> transactionRepository,
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<Warehouse> warehouseRepository,
            IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        public async Task<StockTransactionDto> Handle(CreateStockTransactionCommand request, CancellationToken cancellationToken)
        {
            // Validate Product exists
            var productExists = await _productRepository.GetAll()
                .AnyAsync(p => p.Id == request.Data.ProductId, cancellationToken);

            if (!productExists)
                throw new NotFoundException("Product not found", FinanceErrorCode.NotFound);

            // Validate Warehouse exists
            var warehouseExists = await _warehouseRepository.GetAll()
                .AnyAsync(w => w.Id == request.Data.WarehouseId, cancellationToken);

            if (!warehouseExists)
                throw new NotFoundException("Warehouse not found", FinanceErrorCode.NotFound);

            // Get current stock level
            var currentStockLevel = await _transactionRepository.GetAll()
                .Where(t => t.ProductId == request.Data.ProductId && t.WarehouseId == request.Data.WarehouseId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => t.StockLevelAfter)
                .FirstOrDefaultAsync(cancellationToken);

            // Map to entity
            var transaction = _mapper.Map<StockTransaction>(request.Data);
            transaction.Id = Guid.NewGuid();

            // Calculate stock level after transaction
            var quantityChange = request.Data.Quantity;
            if (request.Data.TransactionType == StockTransactionType.Sale ||
                request.Data.TransactionType == StockTransactionType.Transfer)
            {
                quantityChange = -quantityChange;
            }

            transaction.StockLevelAfter = currentStockLevel + quantityChange;

            // Validate no negative stock
            if (transaction.StockLevelAfter < 0)
            {
                throw new BusinessLogicException(
                    "Transaction would result in negative stock. Available stock: " + currentStockLevel,
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            // Add transaction
            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.SaveChanges();

            // Return DTO with projection
            var result = await _transactionRepository.GetAll()
                .Where(t => t.Id == transaction.Id)
                .ProjectTo<StockTransactionDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return result!;
        }
    }
}

using AutoMapper;
using Azure.Core;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;
using static Azure.Core.HttpHeader;
using System.Threading;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers
{
    public class BulkCreateStockTransactionsCommandHandler : IRequestHandler<BulkCreateStockTransactionsCommand, List<StockTransactionDto>>
    {
        private readonly IGeneralRepository<StockTransaction> _transactionRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<Warehouse> _warehouseRepository;
        private readonly IMapper _mapper;

        public BulkCreateStockTransactionsCommandHandler(
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

        public async Task<List<StockTransactionDto>> Handle(BulkCreateStockTransactionsCommand request, CancellationToken cancellationToken)
        {
            var transactions = new List<StockTransaction>();
            var createdIds = new List<Guid>();

            // Validate all products and warehouses exist
            var productIds = request.Transactions.Select(t => t.ProductId).Distinct().ToList();
            var warehouseIds = request.Transactions.Select(t => t.WarehouseId).Distinct().ToList();

            var validProducts = await _productRepository.GetAll()
                .Where(p => productIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var validWarehouses = await _warehouseRepository.GetAll()
                .Where(w => warehouseIds.Contains(w.Id))
                .Select(w => w.Id)
                .ToListAsync(cancellationToken);

            var invalidProducts = productIds.Except(validProducts).ToList();
            if (invalidProducts.Any())
            {
                throw new BusinessLogicException(
                    $"Invalid product IDs: {string.Join(", ", invalidProducts)}",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            var invalidWarehouses = warehouseIds.Except(validWarehouses).ToList();
            if (invalidWarehouses.Any())
            {
                throw new BusinessLogicException(
                    $"Invalid warehouse IDs: {string.Join(", ", invalidWarehouses)}",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            // Get current stock levels for all product-warehouse combinations
            var stockLevels = new Dictionary<(Guid ProductId, Guid WarehouseId), decimal>();

            foreach (var txnData in request.Transactions)
            {
                var key = (txnData.ProductId, txnData.WarehouseId);

                if (!stockLevels.ContainsKey(key))
                {
                    var currentStock = await _transactionRepository.GetAll()
                        .Where(t => t.ProductId == txnData.ProductId && t.WarehouseId == txnData.WarehouseId)
                        .OrderByDescending(t => t.CreatedAt)
                        .Select(t => t.StockLevelAfter)
                        .FirstOrDefaultAsync(cancellationToken);

                    stockLevels[key] = currentStock;
                }

                var transaction = _mapper.Map<StockTransaction>(txnData);
                transaction.Id = Guid.NewGuid();

                var quantityChange = txnData.Quantity;
                if (txnData.TransactionType == Common.Enum.Inventory_Enum.StockTransactionType.Sale ||
                    txnData.TransactionType == Common.Enum.Inventory_Enum.StockTransactionType.Transfer)
                {
                    quantityChange = -quantityChange;
                }

                stockLevels[key] += quantityChange;
                transaction.StockLevelAfter = stockLevels[key];

                if (transaction.StockLevelAfter < 0)
                {
                    throw new BusinessLogicException(
                        $"Bulk transaction would result in negative stock for Product ID: {txnData.ProductId}, Warehouse ID: {txnData.WarehouseId}",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError);
                }

                transactions.Add(transaction);
                createdIds.Add(transaction.Id);
            }

            await _transactionRepository.AddRangeAsync(transactions);
            await _transactionRepository.SaveChanges();

            // Return created transactions with projection
            var result = await _transactionRepository.GetAll()
                .Where(t => createdIds.Contains(t.Id))
                .ProjectTo<StockTransactionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return result;
        }
    }
} 

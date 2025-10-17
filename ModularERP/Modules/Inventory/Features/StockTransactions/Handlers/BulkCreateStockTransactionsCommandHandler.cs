using AutoMapper;
using Azure.Core;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;
using static Azure.Core.HttpHeader;
using System.Threading;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;
using ModularERP.Modules.Inventory.Features.StockTransactions.Commends;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Modules.Inventory.Features.Products.Models;
using System.Text.Json;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Handlers
{
    public class BulkCreateStockTransactionsCommandHandler : IRequestHandler<BulkCreateStockTransactionsCommand, List<StockTransactionDto>>
    {
        private readonly IGeneralRepository<StockTransaction> _transactionRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<Warehouse> _warehouseRepository;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepository;
        private readonly IGeneralRepository<ProductStats> _productStatsRepository;
        private readonly IGeneralRepository<ActivityLog> _activityLogRepository;
        private readonly IGeneralRepository<ProductTimeline> _productTimelineRepository;
        private readonly IMapper _mapper;

        public BulkCreateStockTransactionsCommandHandler(
            IGeneralRepository<StockTransaction> transactionRepository,
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<Warehouse> warehouseRepository,
            IGeneralRepository<WarehouseStock> warehouseStockRepository,
            IGeneralRepository<ProductStats> productStatsRepository,
            IGeneralRepository<ActivityLog> activityLogRepository,
            IGeneralRepository<ProductTimeline> productTimelineRepository,
            IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
            _warehouseStockRepository = warehouseStockRepository;
            _productStatsRepository = productStatsRepository;
            _activityLogRepository = activityLogRepository;
            _productTimelineRepository = productTimelineRepository;
            _mapper = mapper;
        }

        public async Task<List<StockTransactionDto>> Handle(BulkCreateStockTransactionsCommand request, CancellationToken cancellationToken)
        {
            var transactions = new List<StockTransaction>();
            var createdIds = new List<Guid>();

            // ✅ Validate all products and warehouses exist
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

            // ✅ Get current stock levels for all product-warehouse combinations
            var stockLevels = new Dictionary<(Guid ProductId, Guid WarehouseId), decimal>();

            // ✅ Track warehouse stock updates
            var warehouseStockUpdates = new Dictionary<(Guid ProductId, Guid WarehouseId), WarehouseStock>();

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
                if (txnData.TransactionType == StockTransactionType.Sale ||
                    txnData.TransactionType == StockTransactionType.Transfer)
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

                // ✅ Update WarehouseStock
                var warehouseStock = await UpdateWarehouseStock(
                    txnData.ProductId,
                    txnData.WarehouseId,
                    quantityChange,
                    txnData.UnitCost,
                    cancellationToken
                );

                warehouseStockUpdates[key] = warehouseStock;

                // ✅ Log activity for each transaction
                await LogActivity(
                    txnData.ProductId,
                    "BulkStockTransaction",
                    null,
                    null,
                    new
                    {
                        TransactionType = txnData.TransactionType.ToString(),
                        Quantity = txnData.Quantity,
                        StockLevelAfter = transaction.StockLevelAfter
                    },
                    $"Bulk stock transaction: {txnData.TransactionType} - Quantity: {txnData.Quantity}",
                    cancellationToken
                );

                // ✅ Add to timeline
                await AddToProductTimeline(
                    txnData.ProductId,
                    txnData.TransactionType.ToString(),
                    txnData.ReferenceId.HasValue ? txnData.ReferenceId.Value.ToString() : null,
                    null,
                    transaction.StockLevelAfter,
                    warehouseStock.AverageUnitCost,
                    $"Bulk {txnData.TransactionType}: {txnData.Quantity} units",
                    cancellationToken
                );
            }

            // ✅ Save all transactions
            await _transactionRepository.AddRangeAsync(transactions);
            await _transactionRepository.SaveChanges();

            // ✅ Update ProductStats for all affected products
            foreach (var kvp in warehouseStockUpdates)
            {
                var productId = kvp.Key.ProductId;
                var companyId = request.Transactions.First(t => t.ProductId == productId).CompanyId;
                var warehouseStock = kvp.Value;

                await UpdateProductStats(
                    productId,
                    companyId,
                    warehouseStock.Quantity,
                    warehouseStock.AverageUnitCost ?? 0,
                    cancellationToken
                );
            }

            // ✅ Return created transactions with projection
            var result = await _transactionRepository.GetAll()
                .Where(t => createdIds.Contains(t.Id))
                .ProjectTo<StockTransactionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return result;
        }

        /// <summary>
        /// ✅ تحديث WarehouseStock
        /// </summary>
        private async Task<WarehouseStock> UpdateWarehouseStock(
            Guid productId,
            Guid warehouseId,
            decimal quantityChange,
            decimal? unitCost,
            CancellationToken cancellationToken)
        {
            var warehouseStock = await _warehouseStockRepository.GetAll()
                .FirstOrDefaultAsync(ws => ws.ProductId == productId && ws.WarehouseId == warehouseId, cancellationToken);

            if (warehouseStock == null)
            {
                warehouseStock = new WarehouseStock
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    Quantity = 0,
                    ReservedQuantity = 0,
                    AvailableQuantity = 0,
                    AverageUnitCost = 0,
                    TotalValue = 0
                };
                await _warehouseStockRepository.AddAsync(warehouseStock);
            }

            warehouseStock.Quantity += quantityChange;
            warehouseStock.AvailableQuantity = warehouseStock.Quantity - (warehouseStock.ReservedQuantity ?? 0);

            if (quantityChange > 0 && unitCost.HasValue && unitCost.Value > 0)
            {
                var oldTotalValue = (warehouseStock.Quantity - quantityChange) * (warehouseStock.AverageUnitCost ?? 0);
                var newTotalValue = oldTotalValue + (quantityChange * unitCost.Value);
                warehouseStock.AverageUnitCost = warehouseStock.Quantity > 0
                    ? newTotalValue / warehouseStock.Quantity
                    : 0;
            }

            warehouseStock.TotalValue = warehouseStock.Quantity * (warehouseStock.AverageUnitCost ?? 0);

            if (quantityChange > 0)
                warehouseStock.LastStockInDate = DateTime.UtcNow;
            else if (quantityChange < 0)
                warehouseStock.LastStockOutDate = DateTime.UtcNow;

            warehouseStock.UpdatedAt = DateTime.UtcNow;

            await _warehouseStockRepository.SaveChanges();

            return warehouseStock;
        }

        /// <summary>
        /// ✅ تحديث ProductStats
        /// </summary>
        private async Task UpdateProductStats(
            Guid productId,
            Guid companyId,
            decimal currentStock,
            decimal avgUnitCost,
            CancellationToken cancellationToken)
        {
            var stats = await _productStatsRepository.GetAll()
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.CompanyId == companyId, cancellationToken);

            if (stats == null)
            {
                stats = new ProductStats
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    CompanyId = companyId,
                    TotalSold = 0,
                    SoldLast28Days = 0,
                    SoldLast7Days = 0,
                    OnHandStock = 0,
                    AvgUnitCost = 0
                };
                await _productStatsRepository.AddAsync(stats);
            }

            // ✅ Recalculate sales stats
            var totalSold = await _transactionRepository.GetAll()
                .Where(t => t.ProductId == productId && t.TransactionType == StockTransactionType.Sale)
                .SumAsync(t => t.Quantity, cancellationToken);
            stats.TotalSold = totalSold;

            var last28Days = await _transactionRepository.GetAll()
                .Where(t => t.ProductId == productId &&
                           t.TransactionType == StockTransactionType.Sale &&
                           t.CreatedAt >= DateTime.UtcNow.AddDays(-28))
                .SumAsync(t => t.Quantity, cancellationToken);
            stats.SoldLast28Days = last28Days;

            var last7Days = await _transactionRepository.GetAll()
                .Where(t => t.ProductId == productId &&
                           t.TransactionType == StockTransactionType.Sale &&
                           t.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .SumAsync(t => t.Quantity, cancellationToken);
            stats.SoldLast7Days = last7Days;

            stats.OnHandStock = currentStock;
            stats.AvgUnitCost = avgUnitCost;
            stats.LastUpdated = DateTime.UtcNow;

            await _productStatsRepository.SaveChanges();
        }

        /// <summary>
        /// ✅ تسجيل النشاط في ActivityLog
        /// </summary>
        private async Task LogActivity(
            Guid productId,
            string actionType,
            Guid? userId,
            object? beforeValues,
            object? afterValues,
            string? description,
            CancellationToken cancellationToken)
        {
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ActionType = actionType,
                UserId = userId,
                BeforeValues = beforeValues != null ? JsonSerializer.Serialize(beforeValues) : null,
                AfterValues = afterValues != null ? JsonSerializer.Serialize(afterValues) : null,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            await _activityLogRepository.AddAsync(activityLog);
            await _activityLogRepository.SaveChanges();
        }

        /// <summary>
        /// ✅ إضافة حدث إلى ProductTimeline
        /// </summary>
        private async Task AddToProductTimeline(
            Guid productId,
            string actionType,
            string? itemReference,
            Guid? userId,
            decimal? stockBalance,
            decimal? averagePrice,
            string? description,
            CancellationToken cancellationToken)
        {
            var timeline = new ProductTimeline
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ActionType = actionType,
                ItemReference = itemReference,
                UserId = userId,
                StockBalance = stockBalance,
                AveragePrice = averagePrice,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            await _productTimelineRepository.AddAsync(timeline);
            await _productTimelineRepository.SaveChanges();
        }
    }
}
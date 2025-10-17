using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.StockTransactions.Commends;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;
using System.Text.Json;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Handlers
{
    public class CreateStockTransactionCommandHandler : IRequestHandler<CreateStockTransactionCommand, StockTransactionDto>
    {
        private readonly IGeneralRepository<StockTransaction> _transactionRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<Warehouse> _warehouseRepository;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepository;
        private readonly IGeneralRepository<ProductStats> _productStatsRepository;
        private readonly IGeneralRepository<ActivityLog> _activityLogRepository;
        private readonly IGeneralRepository<ProductTimeline> _productTimelineRepository;
        private readonly IMapper _mapper;

        public CreateStockTransactionCommandHandler(
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

        public async Task<StockTransactionDto> Handle(CreateStockTransactionCommand request, CancellationToken cancellationToken)
        {
            // ✅ Validate Product exists
            var product = await _productRepository.GetAll()
                .FirstOrDefaultAsync(p => p.Id == request.Data.ProductId, cancellationToken);

            if (product == null)
                throw new NotFoundException("Product not found", FinanceErrorCode.NotFound);

            // ✅ Validate Warehouse exists
            var warehouseExists = await _warehouseRepository.GetAll()
                .AnyAsync(w => w.Id == request.Data.WarehouseId, cancellationToken);

            if (!warehouseExists)
                throw new NotFoundException("Warehouse not found", FinanceErrorCode.NotFound);

            // ✅ Get current stock level from transactions
            var currentStockLevel = await _transactionRepository.GetAll()
                .Where(t => t.ProductId == request.Data.ProductId && t.WarehouseId == request.Data.WarehouseId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => t.StockLevelAfter)
                .FirstOrDefaultAsync(cancellationToken);

            // ✅ Map to entity
            var transaction = _mapper.Map<StockTransaction>(request.Data);
            transaction.Id = Guid.NewGuid();

            // ✅ Calculate stock level after transaction
            var quantityChange = request.Data.Quantity;
            if (request.Data.TransactionType == StockTransactionType.Sale ||
                request.Data.TransactionType == StockTransactionType.Transfer)
            {
                quantityChange = -quantityChange;
            }

            transaction.StockLevelAfter = currentStockLevel + quantityChange;

            // ✅ Validate no negative stock
            if (transaction.StockLevelAfter < 0)
            {
                throw new BusinessLogicException(
                    "Transaction would result in negative stock. Available stock: " + currentStockLevel,
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            // ✅ Add transaction
            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.SaveChanges();

            // ✅ UPDATE WAREHOUSE STOCK
            var warehouseStock = await UpdateWarehouseStock(
                request.Data.ProductId,
                request.Data.WarehouseId,
                quantityChange,
                request.Data.UnitCost,
                cancellationToken
            );

            // ✅ UPDATE PRODUCT STATS
            await UpdateProductStats(
                request.Data.ProductId,
                request.Data.CompanyId,
                request.Data.TransactionType,
                request.Data.Quantity,
                warehouseStock.Quantity,
                warehouseStock.AverageUnitCost ?? 0,
                cancellationToken
            );

            // ✅ LOG ACTIVITY
            await LogActivity(
                request.Data.ProductId,
                "StockTransaction",
                transaction.CreatedByUserId,
                null,
                new
                {
                    TransactionType = request.Data.TransactionType.ToString(),
                    Quantity = request.Data.Quantity,
                    StockLevelAfter = transaction.StockLevelAfter
                },
                $"Stock transaction: {request.Data.TransactionType} - Quantity: {request.Data.Quantity}",
                cancellationToken
            );

            // ✅ ADD TO PRODUCT TIMELINE
            await AddToProductTimeline(
                request.Data.ProductId,
                request.Data.TransactionType.ToString(),
                request.Data.ReferenceId.HasValue ? request.Data.ReferenceId.Value.ToString() : null,
                transaction.CreatedByUserId,
                transaction.StockLevelAfter,
                warehouseStock.AverageUnitCost,
                $"{request.Data.TransactionType}: {request.Data.Quantity} units",
                cancellationToken
            );

            // ✅ Return DTO with projection
            var result = await _transactionRepository.GetAll()
                .Where(t => t.Id == transaction.Id)
                .ProjectTo<StockTransactionDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return result!;
        }

        /// <summary>
        /// ✅ دالة تحديث WarehouseStock بعد كل معاملة
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
        /// ✅ تحديث ProductStats بعد كل معاملة
        /// </summary>
        private async Task UpdateProductStats(
            Guid productId,
            Guid companyId,
            StockTransactionType transactionType,
            decimal quantity,
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

            // ✅ Update stats based on transaction type
            if (transactionType == StockTransactionType.Sale)
            {
                stats.TotalSold += quantity;

                // Update sold last 28 days
                var last28Days = await _transactionRepository.GetAll()
                    .Where(t => t.ProductId == productId &&
                               t.TransactionType == StockTransactionType.Sale &&
                               t.CreatedAt >= DateTime.UtcNow.AddDays(-28))
                    .SumAsync(t => t.Quantity, cancellationToken);
                stats.SoldLast28Days = last28Days;

                // Update sold last 7 days
                var last7Days = await _transactionRepository.GetAll()
                    .Where(t => t.ProductId == productId &&
                               t.TransactionType == StockTransactionType.Sale &&
                               t.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                    .SumAsync(t => t.Quantity, cancellationToken);
                stats.SoldLast7Days = last7Days;
            }

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
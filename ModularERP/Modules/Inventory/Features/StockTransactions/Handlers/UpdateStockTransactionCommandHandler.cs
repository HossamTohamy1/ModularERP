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
    public class UpdateStockTransactionCommandHandler : IRequestHandler<UpdateStockTransactionCommand, StockTransactionDto>
    {
        private readonly IGeneralRepository<StockTransaction> _transactionRepository;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepository;
        private readonly IGeneralRepository<ProductStats> _productStatsRepository;
        private readonly IGeneralRepository<ActivityLog> _activityLogRepository;
        private readonly IGeneralRepository<ProductTimeline> _productTimelineRepository;
        private readonly IMapper _mapper;

        public UpdateStockTransactionCommandHandler(
            IGeneralRepository<StockTransaction> transactionRepository,
            IGeneralRepository<WarehouseStock> warehouseStockRepository,
            IGeneralRepository<ProductStats> productStatsRepository,
            IGeneralRepository<ActivityLog> activityLogRepository,
            IGeneralRepository<ProductTimeline> productTimelineRepository,
            IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _warehouseStockRepository = warehouseStockRepository;
            _productStatsRepository = productStatsRepository;
            _activityLogRepository = activityLogRepository;
            _productTimelineRepository = productTimelineRepository;
            _mapper = mapper;
        }

        public async Task<StockTransactionDto> Handle(UpdateStockTransactionCommand request, CancellationToken cancellationToken)
        {
            var existingTransaction = await _transactionRepository.GetByIDWithTracking(request.Data.Id);

            if (existingTransaction == null)
                throw new NotFoundException("Stock transaction not found", FinanceErrorCode.NotFound);

            // ✅ Store old values for logging
            var oldValues = new
            {
                TransactionType = existingTransaction.TransactionType.ToString(),
                Quantity = existingTransaction.Quantity,
                UnitCost = existingTransaction.UnitCost,
                StockLevelAfter = existingTransaction.StockLevelAfter
            };

            // ✅ Calculate old quantity change
            var oldQuantityChange = existingTransaction.Quantity;
            if (existingTransaction.TransactionType == StockTransactionType.Sale ||
                existingTransaction.TransactionType == StockTransactionType.Transfer)
            {
                oldQuantityChange = -oldQuantityChange;
            }

            // ✅ Get stock level before this transaction
            var previousStockLevel = await _transactionRepository.GetAll()
                .Where(t => t.ProductId == existingTransaction.ProductId &&
                           t.WarehouseId == existingTransaction.WarehouseId &&
                           t.CreatedAt < existingTransaction.CreatedAt)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => t.StockLevelAfter)
                .FirstOrDefaultAsync(cancellationToken);

            // ✅ Update allowed fields
            existingTransaction.TransactionType = request.Data.TransactionType;
            existingTransaction.Quantity = request.Data.Quantity;
            existingTransaction.UnitCost = request.Data.UnitCost;
            existingTransaction.ReferenceType = request.Data.ReferenceType;
            existingTransaction.ReferenceId = request.Data.ReferenceId;
            existingTransaction.UpdatedAt = DateTime.UtcNow;

            // ✅ Recalculate stock level after
            var newQuantityChange = request.Data.Quantity;
            if (request.Data.TransactionType == StockTransactionType.Sale ||
                request.Data.TransactionType == StockTransactionType.Transfer)
            {
                newQuantityChange = -newQuantityChange;
            }

            existingTransaction.StockLevelAfter = previousStockLevel + newQuantityChange;

            // ✅ Validate no negative stock
            if (existingTransaction.StockLevelAfter < 0)
            {
                throw new BusinessLogicException(
                    "Transaction update would result in negative stock",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            await _transactionRepository.SaveChanges();

            // ✅ Adjust warehouse stock (reverse old change, apply new change)
            var stockDifference = newQuantityChange - oldQuantityChange;
            var warehouseStock = await UpdateWarehouseStock(
                existingTransaction.ProductId,
                existingTransaction.WarehouseId,
                stockDifference,
                request.Data.UnitCost,
                cancellationToken
            );

            // ✅ Update ProductStats
            await UpdateProductStats(
                existingTransaction.ProductId,
                existingTransaction.CompanyId,
                warehouseStock.Quantity,
                warehouseStock.AverageUnitCost ?? 0,
                cancellationToken
            );

            // ✅ Log activity
            var newValues = new
            {
                TransactionType = request.Data.TransactionType.ToString(),
                Quantity = request.Data.Quantity,
                UnitCost = request.Data.UnitCost,
                StockLevelAfter = existingTransaction.StockLevelAfter
            };

            await LogActivity(
                existingTransaction.ProductId,
                "UpdateStockTransaction",
                null,
                oldValues,
                newValues,
                $"Updated stock transaction from {oldValues.Quantity} to {newValues.Quantity}",
                cancellationToken
            );

            // ✅ Add to timeline
            await AddToProductTimeline(
                existingTransaction.ProductId,
                "Update",
                existingTransaction.ReferenceId?.ToString(),
                null,
                existingTransaction.StockLevelAfter,
                warehouseStock.AverageUnitCost,
                $"Transaction updated: {request.Data.TransactionType} - {request.Data.Quantity} units",
                cancellationToken
            );

            // ✅ Return updated transaction with projection
            var result = await _transactionRepository.GetAll()
                .Where(t => t.Id == existingTransaction.Id)
                .ProjectTo<StockTransactionDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return result!;
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
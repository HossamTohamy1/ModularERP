using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNItem;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRNItem
{
    public class UpdateGRNItemHandler : IRequestHandler<UpdateGRNItemCommand, ResponseViewModel<GRNLineItemResponseDto>>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineItemRepository;
        private readonly IGeneralRepository<POLineItem> _poLineItemRepository;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepository;
        private readonly FinanceDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateGRNItemHandler> _logger;

        public UpdateGRNItemHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<GRNLineItem> grnLineItemRepository,
            IGeneralRepository<POLineItem> poLineItemRepository,
            IGeneralRepository<WarehouseStock> warehouseStockRepository,
            FinanceDbContext dbContext,
            IMapper mapper,
            ILogger<UpdateGRNItemHandler> logger)
        {
            _grnRepository = grnRepository;
            _grnLineItemRepository = grnLineItemRepository;
            _poLineItemRepository = poLineItemRepository;
            _warehouseStockRepository = warehouseStockRepository;
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<GRNLineItemResponseDto>> Handle(
            UpdateGRNItemCommand request,
            CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation(
                    "🔵 Starting UpdateGRNItem | ItemId: {ItemId} | GRNId: {GRNId}",
                    request.ItemId, request.GRNId);

                // ✅ Step 1: Get existing line item with old quantity
                var existingLineItemData = await _grnLineItemRepository.GetAll()
                    .Where(x => x.Id == request.ItemId && x.GRNId == request.GRNId)
                    .Select(x => new
                    {
                        x.Id,
                        x.GRNId,
                        x.POLineItemId,
                        OldReceivedQuantity = x.ReceivedQuantity,
                        x.Notes
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingLineItemData == null)
                {
                    throw new NotFoundException(
                        $"GRN line item {request.ItemId} not found in GRN {request.GRNId}",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation(
                    "✅ Existing line item found | OldQty: {OldQty} | NewQty: {NewQty}",
                    existingLineItemData.OldReceivedQuantity, request.ReceivedQuantity);

                // Calculate quantity change
                decimal quantityDelta = request.ReceivedQuantity - existingLineItemData.OldReceivedQuantity;

                _logger.LogInformation(
                    "📊 Quantity Delta: {Delta} (Old: {Old} → New: {New})",
                    quantityDelta, existingLineItemData.OldReceivedQuantity, request.ReceivedQuantity);

                // ✅ Step 2: Get GRN data with WarehouseId (using Projection)
                var grnData = await _grnRepository.GetAll()
                    .Where(g => g.Id == request.GRNId)
                    .Select(g => new
                    {
                        g.Id,
                        g.GRNNumber,
                        g.WarehouseId,
                        g.PurchaseOrderId
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (grnData == null)
                {
                    throw new NotFoundException(
                        $"GRN {request.GRNId} not found",
                        FinanceErrorCode.NotFound);
                }

                // ✅ Step 3: Get OLD POLineItem data (for stock reversal if changed)
                var oldPoLineItemData = await _poLineItemRepository.GetAll()
                    .Where(x => x.Id == existingLineItemData.POLineItemId)
                    .Select(x => new
                    {
                        x.Id,
                        x.ProductId,
                        x.ServiceId,
                        x.Quantity,
                        x.ReceivedQuantity,
                        x.UnitPrice,
                        ProductName = x.Product != null ? x.Product.Name : null,
                        ServiceName = x.Service != null ? x.Service.Name : null,
                        x.Description
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                // ✅ Step 4: Get NEW POLineItem data (using Projection)
                var newPoLineItemData = await _poLineItemRepository.GetAll()
                    .Where(x => x.Id == request.POLineItemId)
                    .Select(x => new
                    {
                        x.Id,
                        x.PurchaseOrderId,
                        x.ProductId,
                        x.ServiceId,
                        x.Quantity,
                        x.ReceivedQuantity,
                        x.RemainingQuantity,
                        x.UnitPrice,
                        x.Description,
                        ProductName = x.Product != null ? x.Product.Name : null,
                        ProductSKU = x.Product != null ? x.Product.SKU : null,
                        ServiceName = x.Service != null ? x.Service.Name : null
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (newPoLineItemData == null)
                {
                    throw new NotFoundException(
                        $"PO Line Item {request.POLineItemId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Validate belongs to same PO
                if (newPoLineItemData.PurchaseOrderId != grnData.PurchaseOrderId)
                {
                    throw new BusinessLogicException(
                        $"Line item does not belong to the same Purchase Order",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                string itemName = newPoLineItemData.ProductName
                    ?? newPoLineItemData.ServiceName
                    ?? newPoLineItemData.Description
                    ?? "Unknown Item";

                // ✅ Step 5: Validate new quantity
                if (request.ReceivedQuantity <= 0)
                {
                    throw new BusinessLogicException(
                        $"Received quantity must be greater than zero",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                // Check if POLineItem changed
                bool poLineItemChanged = existingLineItemData.POLineItemId != request.POLineItemId;

                if (poLineItemChanged)
                {
                    _logger.LogInformation(
                        "⚠️ POLineItem changed | Old: {OldId} | New: {NewId}",
                        existingLineItemData.POLineItemId, request.POLineItemId);

                    // Validate new POLineItem can accept the quantity
                    decimal newPendingQty = newPoLineItemData.Quantity - newPoLineItemData.ReceivedQuantity;

                    if (request.ReceivedQuantity > newPendingQty)
                    {
                        throw new BusinessLogicException(
                            $"Cannot receive {request.ReceivedQuantity} units of '{itemName}'. " +
                            $"Only {newPendingQty} units pending",
                            "Purchases",
                            FinanceErrorCode.ValidationError);
                    }
                }
                else
                {
                    // Same POLineItem - validate considering the delta
                    decimal currentPending = newPoLineItemData.Quantity - newPoLineItemData.ReceivedQuantity;
                    decimal adjustedPending = currentPending + existingLineItemData.OldReceivedQuantity;

                    if (request.ReceivedQuantity > adjustedPending)
                    {
                        throw new BusinessLogicException(
                            $"Cannot receive {request.ReceivedQuantity} units of '{itemName}'. " +
                            $"Maximum allowed is {adjustedPending} units",
                            "Purchases",
                            FinanceErrorCode.ValidationError);
                    }
                }

                // ✅ Step 6: Update GRN Line Item
                var lineItem = await _grnLineItemRepository.GetByIDWithTracking(request.ItemId);
                if (lineItem != null)
                {
                    lineItem.POLineItemId = request.POLineItemId;
                    lineItem.ReceivedQuantity = request.ReceivedQuantity;
                    lineItem.Notes = request.Notes;
                    lineItem.UpdatedById = request.UserId;
                    lineItem.UpdatedAt = DateTime.UtcNow;

                    await _grnLineItemRepository.SaveChanges();

                    _logger.LogInformation("✅ GRN Line Item updated | Id: {LineItemId}", request.ItemId);
                }

                // ✅ Step 7: Update OLD POLineItem (if changed)
                if (poLineItemChanged && oldPoLineItemData != null)
                {
                    var oldPoLineItem = await _poLineItemRepository.GetByIDWithTracking(existingLineItemData.POLineItemId);
                    if (oldPoLineItem != null)
                    {
                        decimal oldReceived = oldPoLineItem.ReceivedQuantity;
                        oldPoLineItem.ReceivedQuantity -= existingLineItemData.OldReceivedQuantity;
                        oldPoLineItem.RemainingQuantity = oldPoLineItem.Quantity - oldPoLineItem.ReceivedQuantity;
                        oldPoLineItem.UpdatedAt = DateTime.UtcNow;
                        oldPoLineItem.UpdatedById = request.UserId;

                        await _poLineItemRepository.SaveChanges();

                        _logger.LogInformation(
                            "✅ OLD POLineItem updated | Id: {POLineItemId} | " +
                            "Received: {OldReceived} → {NewReceived}",
                            existingLineItemData.POLineItemId, oldReceived, oldPoLineItem.ReceivedQuantity);

                        // Reverse stock for old product (if it was a product)
                        if (oldPoLineItemData.ProductId.HasValue)
                        {
                            await AdjustWarehouseStock(
                                grnData.WarehouseId,
                                oldPoLineItemData.ProductId.Value,
                                -existingLineItemData.OldReceivedQuantity, // Negative to reverse
                                oldPoLineItemData.UnitPrice,
                                oldPoLineItemData.ProductName ?? "Unknown",
                          //      request.UserId,
                                cancellationToken);
                        }
                    }
                }

                // ✅ Step 8: Update NEW POLineItem
                var newPoLineItem = await _poLineItemRepository.GetByIDWithTracking(request.POLineItemId);
                if (newPoLineItem != null)
                {
                    decimal oldReceived = newPoLineItem.ReceivedQuantity;

                    if (poLineItemChanged)
                    {
                        // Add full new quantity
                        newPoLineItem.ReceivedQuantity += request.ReceivedQuantity;
                    }
                    else
                    {
                        // Apply delta
                        newPoLineItem.ReceivedQuantity += quantityDelta;
                    }

                    newPoLineItem.RemainingQuantity = newPoLineItem.Quantity - newPoLineItem.ReceivedQuantity;
                    newPoLineItem.UpdatedAt = DateTime.UtcNow;
                    newPoLineItem.UpdatedById = request.UserId;

                    await _poLineItemRepository.SaveChanges();

                    _logger.LogInformation(
                        "✅ NEW POLineItem updated | Id: {POLineItemId} | " +
                        "Received: {OldReceived} → {NewReceived} | Remaining: {Remaining}",
                        request.POLineItemId, oldReceived, newPoLineItem.ReceivedQuantity,
                        newPoLineItem.RemainingQuantity);
                }

                // ✅ Step 9: Update Warehouse Stock for NEW product
                if (newPoLineItemData.ProductId.HasValue)
                {
                    decimal stockAdjustment = poLineItemChanged
                        ? request.ReceivedQuantity // Full quantity if POLineItem changed
                        : quantityDelta; // Delta if same POLineItem

                    await AdjustWarehouseStock(
                        grnData.WarehouseId,
                        newPoLineItemData.ProductId.Value,
                        stockAdjustment,
                        newPoLineItemData.UnitPrice,
                        itemName,
                      //  request.UserId,
                        cancellationToken);
                }
                else
                {
                    _logger.LogInformation(
                        "⏭️ Skipping stock update for Service: '{ItemName}'",
                        itemName);
                }

                // ✅ Step 10: Commit Transaction
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "✅✅✅ GRN Item updated successfully | ItemId: {ItemId}",
                    request.ItemId);

                // ✅ Step 11: Build Response using Projection
                var result = await _grnLineItemRepository.GetAll()
                    .Where(l => l.Id == request.ItemId)
                    .Select(l => new GRNLineItemResponseDto
                    {
                        Id = l.Id,
                        GRNId = l.GRNId,
                        POLineItemId = l.POLineItemId,
                        ProductId = l.POLineItem.ProductId,
                        ProductName = l.POLineItem.Product != null
                            ? l.POLineItem.Product.Name
                            : (l.POLineItem.Service != null
                                ? l.POLineItem.Service.Name
                                : l.POLineItem.Description),
                        ProductSKU = l.POLineItem.Product != null
                            ? l.POLineItem.Product.SKU
                            : null,
                        OrderedQuantity = l.POLineItem.Quantity,
                        PreviouslyReceivedQuantity = l.POLineItem.ReceivedQuantity - l.ReceivedQuantity,
                        ReceivedQuantity = l.ReceivedQuantity,
                        RemainingQuantity = l.POLineItem.RemainingQuantity,
                        IsFullyReceived = l.POLineItem.RemainingQuantity == 0,
                        UnitOfMeasure = null,
                        Notes = l.Notes
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return ResponseViewModel<GRNLineItemResponseDto>.Success(
                    result!,
                    "GRN Item updated successfully and stock adjusted");
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                await transaction.RollbackAsync(cancellationToken);

                _logger.LogError(ex,
                    "❌ Error updating GRN item | ItemId: {ItemId} | GRNId: {GRNId}",
                    request.ItemId, request.GRNId);

                throw new BusinessLogicException(
                    $"Error updating GRN item: {ex.Message}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task AdjustWarehouseStock(
            Guid warehouseId,
            Guid productId,
            decimal quantityAdjustment,
            decimal unitPrice,
            string productName,
          //  Guid? userId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "🔄 Adjusting stock | Warehouse: {WarehouseId} | Product: {ProductId} | " +
                "Adjustment: {Adjustment} ({Direction})",
                warehouseId, productId, Math.Abs(quantityAdjustment),
                quantityAdjustment >= 0 ? "ADD" : "REMOVE");

            var existingStock = await _warehouseStockRepository.GetAll()
                .Where(x => x.WarehouseId == warehouseId && x.ProductId == productId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingStock != null)
            {
                decimal oldQty = existingStock.Quantity;
                decimal oldAvailable = existingStock.AvailableQuantity;

                existingStock.Quantity += quantityAdjustment;
                existingStock.AvailableQuantity += quantityAdjustment;

                // Validate stock doesn't go negative
                if (existingStock.Quantity < 0)
                {
                    throw new BusinessLogicException(
                        $"Cannot adjust stock. Would result in negative quantity for '{productName}'",
                        "Inventory",
                        FinanceErrorCode.ValidationError);
                }

                existingStock.LastStockInDate = quantityAdjustment > 0
                    ? DateTime.UtcNow
                    : existingStock.LastStockInDate;
                existingStock.UpdatedAt = DateTime.UtcNow;
                existingStock.UpdatedById = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");
                existingStock.TotalValue = existingStock.Quantity * (existingStock.AverageUnitCost ?? unitPrice);

                await _warehouseStockRepository.SaveChanges();

                _logger.LogInformation(
                    "✅ Stock adjusted | Product: '{ProductName}' | " +
                    "Qty: {OldQty} → {NewQty} | Available: {OldAvailable} → {NewAvailable}",
                    productName, oldQty, existingStock.Quantity,
                    oldAvailable, existingStock.AvailableQuantity);
            }
            else if (quantityAdjustment > 0)
            {
                // Create new stock record (only if adding stock)
                var newStock = new WarehouseStock
                {
                    WarehouseId = warehouseId,
                    ProductId = productId,
                    Quantity = quantityAdjustment,
                    AvailableQuantity = quantityAdjustment,
                    ReservedQuantity = 0,
                    AverageUnitCost = unitPrice,
                    TotalValue = unitPrice * quantityAdjustment,
                    LastStockInDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53"),
                    IsActive = true,
                    IsDeleted = false
                };

                await _warehouseStockRepository.AddAsync(newStock);
                await _warehouseStockRepository.SaveChanges();

                _logger.LogInformation(
                    "✅ New stock created | Product: '{ProductName}' | Initial Qty: {Qty}",
                    productName, quantityAdjustment);
            }
            else
            {
                throw new BusinessLogicException(
                    $"Cannot remove stock for '{productName}'. No existing stock record found",
                    "Inventory",
                    FinanceErrorCode.NotFound);
            }
        }
    }
}
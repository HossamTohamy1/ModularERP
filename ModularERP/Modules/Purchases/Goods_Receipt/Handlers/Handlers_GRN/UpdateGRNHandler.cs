using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRN
{
    public class UpdateGRNHandler : IRequestHandler<UpdateGRNCommand, ResponseViewModel<GRNResponseDto>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<GRNLineItem> _lineItemRepository;
        private readonly IGeneralRepository<POLineItem> _poLineItemRepository;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepository; // ✅ إضافة
        private readonly FinanceDbContext _dbContext; // ✅ إضافة للـ Transaction
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateGRNHandler> _logger;

        public UpdateGRNHandler(
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<GRNLineItem> lineItemRepository,
            IGeneralRepository<POLineItem> poLineItemRepository,
            IGeneralRepository<WarehouseStock> warehouseStockRepository, // ✅ إضافة
            FinanceDbContext dbContext, // ✅ إضافة
            IMapper mapper,
            ILogger<UpdateGRNHandler> logger)
        {
            _poRepository = poRepository;
            _grnRepository = grnRepository;
            _lineItemRepository = lineItemRepository;
            _poLineItemRepository = poLineItemRepository;
            _warehouseStockRepository = warehouseStockRepository; // ✅ إضافة
            _dbContext = dbContext; // ✅ إضافة
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<GRNResponseDto>> Handle(UpdateGRNCommand request, CancellationToken cancellationToken)
        {
            // ✅ استخدام Transaction
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation("🔄 Updating GRN with ID: {GRNId}", request.Data.Id);

                // ✅ 1. Get existing GRN
                var existingGrn = await _grnRepository.GetByIDWithTracking(request.Data.Id);
                if (existingGrn == null)
                    throw new NotFoundException(
                        $"GRN with ID {request.Data.Id} not found",
                        FinanceErrorCode.NotFound);

                _logger.LogInformation("Existing GRN found: {GRNNumber} | PO: {POId}",
                    existingGrn.GRNNumber, existingGrn.PurchaseOrderId);

                // ✅ 2. Get current PO status
                var po = await _poRepository.GetAll()
                    .Where(x => x.Id == existingGrn.PurchaseOrderId)
                    .Select(x => new
                    {
                        x.Id,
                        x.PONumber,
                        x.ReceptionStatus,
                        x.PaymentStatus,
                        x.DocumentStatus
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (po == null)
                    throw new NotFoundException(
                        $"Purchase Order with ID {existingGrn.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);

                string previousReceptionStatus = po.ReceptionStatus.ToString();

                // ✅ 3. Get existing line items with FULL details including ProductId
                var existingLineItems = await _lineItemRepository.GetAll()
                    .Include(x => x.POLineItem)
                    .ThenInclude(x => x.Product)
                    .Where(x => x.GRNId == request.Data.Id && !x.IsDeleted)
                    .Select(x => new
                    {
                        x.Id,
                        x.POLineItemId,
                        x.ReceivedQuantity,
                        ProductId = x.POLineItem.ProductId,
                        ServiceId = x.POLineItem.ServiceId,
                        ProductName = x.POLineItem.Product != null ? x.POLineItem.Product.Name : null
                    })
                    .ToListAsync(cancellationToken);

                // ✅ 4. Calculate quantity changes for POLineItems
                var quantityChanges = new Dictionary<Guid, decimal>(); // POLineItemId -> Delta

                // ✅ 5. Calculate stock adjustments (for WarehouseStock)
                // Structure: (WarehouseId, ProductId) -> QuantityDelta
                var stockAdjustments = new Dictionary<(Guid warehouseId, Guid productId), decimal>();

                // ✅ 6. Identify deleted line items
                var lineItemIdsToKeep = request.Data.LineItems
                    .Where(x => x.Id.HasValue)
                    .Select(x => x.Id!.Value)
                    .ToList();

                var lineItemsToDelete = existingLineItems
                    .Where(x => !lineItemIdsToKeep.Contains(x.Id))
                    .ToList();

                foreach (var deletedItem in lineItemsToDelete)
                {
                    // Subtract from POLineItem ReceivedQuantity
                    if (!quantityChanges.ContainsKey(deletedItem.POLineItemId))
                        quantityChanges[deletedItem.POLineItemId] = 0;

                    quantityChanges[deletedItem.POLineItemId] -= deletedItem.ReceivedQuantity;

                    // Subtract from WarehouseStock (return to stock)
                    if (deletedItem.ProductId.HasValue)
                    {
                        var key = (existingGrn.WarehouseId, deletedItem.ProductId.Value);
                        if (!stockAdjustments.ContainsKey(key))
                            stockAdjustments[key] = 0;

                        stockAdjustments[key] -= deletedItem.ReceivedQuantity;

                        _logger.LogInformation(
                            "🗑️ Line item deleted | GRNLineItemId: {Id} | Product: '{ProductName}' | " +
                            "Qty to subtract from stock: {Qty}",
                            deletedItem.Id, deletedItem.ProductName, deletedItem.ReceivedQuantity);
                    }
                }

                // ✅ 7. Calculate updates and additions
                foreach (var lineItemDto in request.Data.LineItems)
                {
                    if (!quantityChanges.ContainsKey(lineItemDto.POLineItemId))
                        quantityChanges[lineItemDto.POLineItemId] = 0;

                    // Get POLineItem info for stock adjustment
                    var poLineItemInfo = await _poLineItemRepository.GetAll()
                        .Where(x => x.Id == lineItemDto.POLineItemId)
                        .Select(x => new
                        {
                            x.Id,
                            x.ProductId,
                            x.ServiceId,
                            ProductName = x.Product != null ? x.Product.Name : null
                        })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (lineItemDto.Id.HasValue)
                    {
                        // ✅ Updated line item - calculate delta
                        var existing = existingLineItems.FirstOrDefault(x => x.Id == lineItemDto.Id.Value);
                        if (existing != null)
                        {
                            decimal delta = lineItemDto.ReceivedQuantity - existing.ReceivedQuantity;
                            quantityChanges[lineItemDto.POLineItemId] += delta;

                            // Adjust stock by delta
                            if (poLineItemInfo?.ProductId.HasValue == true)
                            {
                                var key = (existingGrn.WarehouseId, poLineItemInfo.ProductId.Value);
                                if (!stockAdjustments.ContainsKey(key))
                                    stockAdjustments[key] = 0;

                                stockAdjustments[key] += delta;

                                _logger.LogInformation(
                                    "📝 Line item updated | GRNLineItemId: {Id} | Product: '{ProductName}' | " +
                                    "Old: {Old} | New: {New} | Delta: {Delta}",
                                    lineItemDto.Id.Value, poLineItemInfo.ProductName,
                                    existing.ReceivedQuantity, lineItemDto.ReceivedQuantity, delta);
                            }
                        }
                    }
                    else
                    {
                        // ✅ New line item - add full quantity
                        quantityChanges[lineItemDto.POLineItemId] += lineItemDto.ReceivedQuantity;

                        // Add to stock
                        if (poLineItemInfo?.ProductId.HasValue == true)
                        {
                            var key = (existingGrn.WarehouseId, poLineItemInfo.ProductId.Value);
                            if (!stockAdjustments.ContainsKey(key))
                                stockAdjustments[key] = 0;

                            stockAdjustments[key] += lineItemDto.ReceivedQuantity;

                            _logger.LogInformation(
                                "➕ Line item added | POLineItemId: {POLineItemId} | Product: '{ProductName}' | " +
                                "Qty to add to stock: {Qty}",
                                lineItemDto.POLineItemId, poLineItemInfo.ProductName, lineItemDto.ReceivedQuantity);
                        }
                    }
                }

                // ✅ 8. Validate quantities before applying changes
                foreach (var change in quantityChanges)
                {
                    var poLineItem = await _poLineItemRepository.GetAll()
                        .Where(x => x.Id == change.Key)
                        .Select(x => new
                        {
                            x.Id,
                            x.Quantity,
                            x.ReceivedQuantity,
                            ProductName = x.Product != null ? x.Product.Name : null,
                            ServiceName = x.Service != null ? x.Service.Name : null,
                            x.Description
                        })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (poLineItem == null)
                        throw new NotFoundException(
                            $"PO Line Item {change.Key} not found",
                            FinanceErrorCode.NotFound);

                    decimal newReceivedQty = poLineItem.ReceivedQuantity + change.Value;

                    if (newReceivedQty < 0)
                    {
                        string itemName = poLineItem.ProductName ?? poLineItem.ServiceName ??
                                         poLineItem.Description ?? "Unknown Item";
                        throw new BusinessLogicException(
                            $"Cannot update GRN. The change would result in negative received quantity for '{itemName}'.",
                            module: "Purchases",
                            financeErrorCode: FinanceErrorCode.ValidationError);
                    }

                    if (newReceivedQty > poLineItem.Quantity)
                    {
                        string itemName = poLineItem.ProductName ?? poLineItem.ServiceName ??
                                         poLineItem.Description ?? "Unknown Item";
                        throw new BusinessLogicException(
                            $"Cannot update GRN. Total received ({newReceivedQty}) would exceed ordered ({poLineItem.Quantity}) for '{itemName}'.",
                            module: "Purchases",
                            financeErrorCode: FinanceErrorCode.ValidationError);
                    }
                }

                // ✅ 9. Validate stock adjustments (prevent negative stock)
                foreach (var adjustment in stockAdjustments)
                {
                    var (warehouseId, productId) = adjustment.Key;
                    var delta = adjustment.Value;

                    if (delta < 0) // Only check when reducing stock
                    {
                        var currentStock = await _warehouseStockRepository.GetAll()
                            .Where(x => x.WarehouseId == warehouseId && x.ProductId == productId)
                            .Select(x => new { x.Quantity, x.AvailableQuantity })
                            .FirstOrDefaultAsync(cancellationToken);

                        if (currentStock == null)
                            throw new BusinessLogicException(
                                $"Cannot reduce stock. No stock record found for ProductId: {productId}",
                                module: "Purchases",
                                financeErrorCode: FinanceErrorCode.ValidationError);

                        if (currentStock.Quantity + delta < 0)
                            throw new BusinessLogicException(
                                $"Cannot reduce stock by {Math.Abs(delta)}. Current stock: {currentStock.Quantity}",
                                module: "Purchases",
                                financeErrorCode: FinanceErrorCode.ValidationError);
                    }
                }

                // ✅ 10. Apply quantity changes to POLineItems
                foreach (var change in quantityChanges)
                {
                    if (change.Value != 0)
                    {
                        var poLineItem = await _poLineItemRepository.GetByIDWithTracking(change.Key);
                        if (poLineItem != null)
                        {
                            decimal oldQty = poLineItem.ReceivedQuantity;
                            poLineItem.ReceivedQuantity += change.Value;
                            poLineItem.RemainingQuantity = poLineItem.Quantity - poLineItem.ReceivedQuantity;
                            await _poLineItemRepository.SaveChanges();

                            _logger.LogInformation(
                                "✅ POLineItem updated | Id: {Id} | Old: {Old} | Change: {Change} | New: {New}",
                                change.Key, oldQty, change.Value, poLineItem.ReceivedQuantity);
                        }
                    }
                }

                // ✅ 11. **Apply stock adjustments to WarehouseStock**
                _logger.LogInformation("🔄 Applying {Count} stock adjustment(s)...", stockAdjustments.Count);

                foreach (var adjustment in stockAdjustments)
                {
                    var (warehouseId, productId) = adjustment.Key;
                    var delta = adjustment.Value;

                    if (delta == 0)
                        continue; // No change needed

                    var stock = await _warehouseStockRepository.GetAll()
                        .Where(x => x.WarehouseId == warehouseId && x.ProductId == productId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (stock != null)
                    {
                        // Update existing stock
                        decimal previousQty = stock.Quantity;
                        stock.Quantity += delta;
                        stock.AvailableQuantity += delta;
                        stock.LastStockInDate = DateTime.UtcNow;
                        stock.UpdatedAt = DateTime.UtcNow;

                        // Recalculate TotalValue if needed
                        if (stock.AverageUnitCost.HasValue)
                            stock.TotalValue = stock.Quantity * stock.AverageUnitCost.Value;

                        await _warehouseStockRepository.SaveChanges();

                        _logger.LogInformation(
                            "✅ Stock adjusted | ProductId: {ProductId} | Warehouse: {WarehouseId} | " +
                            "Previous: {Previous} | Delta: {Delta} | New: {New}",
                            productId, warehouseId, previousQty, delta, stock.Quantity);
                    }
                    else if (delta > 0)
                    {
                        // Create new stock record (only for positive adjustments)
                        var newStock = new WarehouseStock
                        {
                            WarehouseId = warehouseId,
                            ProductId = productId,
                            Quantity = delta,
                            AvailableQuantity = delta,
                            ReservedQuantity = 0,
                            LastStockInDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true,
                            IsDeleted = false
                        };

                        await _warehouseStockRepository.AddAsync(newStock);
                        await _warehouseStockRepository.SaveChanges();

                        _logger.LogInformation(
                            "✅ New stock created | ProductId: {ProductId} | Warehouse: {WarehouseId} | Qty: {Qty}",
                            productId, warehouseId, delta);
                    }
                }

                _logger.LogInformation("✅ Stock adjustments completed successfully");

                // ✅ 12. Update PO Reception Status
                var allPoLineItems = await _poLineItemRepository.GetAll()
                    .Where(x => x.PurchaseOrderId == existingGrn.PurchaseOrderId)
                    .ToListAsync(cancellationToken);

                string newReceptionStatus = CalculateReceptionStatus(allPoLineItems);

                if (newReceptionStatus != previousReceptionStatus)
                {
                    var purchaseOrder = await _poRepository.GetByIDWithTracking(existingGrn.PurchaseOrderId);
                    if (purchaseOrder != null)
                    {
                        purchaseOrder.ReceptionStatus = Enum.Parse<ReceptionStatus>(newReceptionStatus);
                        await _poRepository.SaveChanges();

                        _logger.LogInformation(
                            "✅ PO Reception Status updated | PO: {PONumber} | Previous: {Previous} | New: {New}",
                            po.PONumber, previousReceptionStatus, newReceptionStatus);
                    }
                }

                // ✅ 13. Update GRN main properties
                existingGrn.WarehouseId = request.Data.WarehouseId;
                existingGrn.ReceiptDate = request.Data.ReceiptDate;
                existingGrn.ReceivedBy = request.Data.ReceivedBy;
                existingGrn.Notes = request.Data.Notes;
                existingGrn.UpdatedAt = DateTime.UtcNow;

                // ✅ 14. Delete removed line items
                foreach (var lineItem in lineItemsToDelete)
                {
                    await _lineItemRepository.Delete(lineItem.Id);
                }

                // ✅ 15. Update or add line items
                foreach (var lineItemDto in request.Data.LineItems)
                {
                    if (lineItemDto.Id.HasValue)
                    {
                        var existingLineItem = await _lineItemRepository.GetByIDWithTracking(lineItemDto.Id.Value);
                        if (existingLineItem != null)
                        {
                            existingLineItem.POLineItemId = lineItemDto.POLineItemId;
                            existingLineItem.ReceivedQuantity = lineItemDto.ReceivedQuantity;
                            existingLineItem.Notes = lineItemDto.Notes;
                            existingLineItem.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        var newLineItem = new GRNLineItem
                        {
                            GRNId = request.Data.Id,
                            POLineItemId = lineItemDto.POLineItemId,
                            ReceivedQuantity = lineItemDto.ReceivedQuantity,
                            Notes = lineItemDto.Notes
                        };
                        await _lineItemRepository.AddAsync(newLineItem);
                    }
                }

                await _grnRepository.SaveChanges();

                // ✅ Commit Transaction
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("✅✅✅ GRN updated successfully: {GRNNumber}", existingGrn.GRNNumber);

                // ✅ 16. Build response
                var response = await BuildGRNResponse(
                    request.Data.Id,
                    previousReceptionStatus,
                    newReceptionStatus,
                    cancellationToken);

                return ResponseViewModel<GRNResponseDto>.Success(
                    response,
                    "GRN updated successfully and inventory adjusted");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "❌ Error updating GRN. Transaction rolled back. GRNId: {GRNId}",
                    request.Data.Id);
                throw;
            }
        }

        private async Task<GRNResponseDto> BuildGRNResponse(
            Guid grnId,
            string previousReceptionStatus,
            string currentReceptionStatus,
            CancellationToken cancellationToken)
        {
            var grn = await _grnRepository.GetAll()
                .Where(x => x.Id == grnId)
                .Select(g => new
                {
                    g.Id,
                    g.GRNNumber,
                    g.PurchaseOrderId,
                    PONumber = g.PurchaseOrder.PONumber,
                    POReceptionStatus = g.PurchaseOrder.ReceptionStatus,
                    POPaymentStatus = g.PurchaseOrder.PaymentStatus,
                    PODocumentStatus = g.PurchaseOrder.DocumentStatus,
                    g.WarehouseId,
                    WarehouseName = g.Warehouse.Name,
                    g.ReceiptDate,
                    g.ReceivedBy,
                    g.Notes,
                    g.CompanyId,
                    g.CreatedAt,
                    g.CreatedById,
                    CreatedByName = g.CreatedByUser != null
                        ? $"{g.CreatedByUser.FirstName} {g.CreatedByUser.LastName}"
                        : null,
                    LineItems = g.LineItems
                        .Where(li => !li.IsDeleted)
                        .Select(li => new
                        {
                            li.Id,
                            li.POLineItemId,
                            li.POLineItem.ProductId,
                            li.POLineItem.ServiceId,
                            li.POLineItem.Description,
                            ProductName = li.POLineItem.Product != null ? li.POLineItem.Product.Name : null,
                            ProductSKU = li.POLineItem.Product != null ? li.POLineItem.Product.SKU : null,
                            ServiceName = li.POLineItem.Service != null ? li.POLineItem.Service.Name : null,
                            OrderedQty = li.POLineItem.Quantity,
                            ReceivedQty = li.POLineItem.ReceivedQuantity,
                            CurrentReceived = li.ReceivedQuantity,
                            li.Notes
                        }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (grn == null)
                throw new NotFoundException("GRN not found after update", FinanceErrorCode.NotFound);

            decimal totalRemaining = 0;
            var lineItemsResponse = new List<GRNLineItemResponseDto>();
            var inventoryImpact = new List<InventoryImpactDto>();

            foreach (var li in grn.LineItems)
            {
                decimal previousReceived = li.ReceivedQty - li.CurrentReceived;
                decimal remaining = li.OrderedQty - li.ReceivedQty;
                totalRemaining += remaining;

                string displayName = li.ProductName ?? li.ServiceName ?? li.Description ?? "Unknown Item";
                displayName = string.IsNullOrWhiteSpace(displayName) ? "Unknown Item" : displayName.Trim();
                string displaySKU = string.IsNullOrWhiteSpace(li.ProductSKU) ? "" : li.ProductSKU.Trim();

                lineItemsResponse.Add(new GRNLineItemResponseDto
                {
                    Id = li.Id,
                    POLineItemId = li.POLineItemId,
                    ProductId = li.ProductId,
                    ProductName = displayName,
                    ProductSKU = displaySKU,
                    OrderedQuantity = li.OrderedQty,
                    PreviouslyReceivedQuantity = previousReceived,
                    ReceivedQuantity = li.CurrentReceived,
                    RemainingQuantity = remaining,
                    IsFullyReceived = remaining == 0,
                    UnitOfMeasure = null,
                    Notes = li.Notes
                });

                if (li.ProductId.HasValue)
                {
                    inventoryImpact.Add(new InventoryImpactDto
                    {
                        ProductId = li.ProductId.Value,
                        ProductName = displayName,
                        ProductSKU = displaySKU,
                        WarehouseId = grn.WarehouseId,
                        WarehouseName = grn.WarehouseName,
                        QuantityAdded = li.CurrentReceived,
                        PreviousStock = 0,
                        NewStock = li.CurrentReceived
                    });
                }
            }

            var warnings = new List<string>();
            if (totalRemaining > 0)
                warnings.Add($"{totalRemaining} units still pending receipt");

            if (grn.POPaymentStatus != PaymentStatus.PaidInFull && grn.POPaymentStatus != PaymentStatus.Refunded)
                warnings.Add($"Payment status: '{grn.POPaymentStatus}'. Full payment required before closing PO.");

            return new GRNResponseDto
            {
                Id = grn.Id,
                GRNNumber = grn.GRNNumber,
                PurchaseOrderId = grn.PurchaseOrderId,
                PurchaseOrderNumber = grn.PONumber,
                WarehouseId = grn.WarehouseId,
                WarehouseName = grn.WarehouseName,
                ReceiptDate = grn.ReceiptDate,
                ReceivedBy = grn.ReceivedBy,
                Notes = grn.Notes,
                CompanyId = grn.CompanyId,
                CreatedAt = grn.CreatedAt,
                CreatedById = grn.CreatedById,
                CreatedByName = grn.CreatedByName,
                PurchaseOrderStatus = new POStatusInfo
                {
                    ReceptionStatus = currentReceptionStatus,
                    PaymentStatus = grn.POPaymentStatus.ToString() ?? "Unknown",
                    DocumentStatus = grn.PODocumentStatus.ToString() ?? "Unknown",
                    PreviousReceptionStatus = previousReceptionStatus
                },
                LineItems = lineItemsResponse,
                InventoryImpact = inventoryImpact,
                NextActions = new GRNNextActionsDto
                {
                    CanReceiveMore = totalRemaining > 0,
                    CanCreateReturn = true,
                    CanCreateInvoice = true,
                    CanClose = currentReceptionStatus == "Fully Received" && grn.POPaymentStatus == PaymentStatus.PaidInFull,
                    TotalRemainingToReceive = totalRemaining,
                    Warnings = warnings
                }
            };
        }

        private string CalculateReceptionStatus(List<POLineItem> lineItems)
        {
            if (lineItems.All(x => x.ReceivedQuantity == 0))
                return "Not Received";

            if (lineItems.All(x => x.ReceivedQuantity >= x.Quantity))
                return "Fully Received";

            return "Partially Received";
        }
    }
}
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNPO;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRNPO
{
    public class ReceiveFromPOHandler : IRequestHandler<ReceiveFromPOCommand, GRNResponseDto>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IGeneralRepository<POLineItem> _poLineItemRepository;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepository;
        private readonly FinanceDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<ReceiveFromPOHandler> _logger;

        public ReceiveFromPOHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<POLineItem> poLineItemRepository,
            IGeneralRepository<WarehouseStock> warehouseStockRepository,
            FinanceDbContext dbContext,
            IMapper mapper,
            ILogger<ReceiveFromPOHandler> logger)
        {
            _grnRepository = grnRepository;
            _poRepository = poRepository;
            _poLineItemRepository = poLineItemRepository;
            _warehouseStockRepository = warehouseStockRepository;
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GRNResponseDto> Handle(ReceiveFromPOCommand request, CancellationToken cancellationToken)
        {
            // ✅ Start Transaction
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation("🔵 Creating GRN for PO {PurchaseOrderId}", request.PurchaseOrderId);

                // ✅ 1. Validate Purchase Order
                var po = await _poRepository.GetAll()
                    .Where(x => x.Id == request.PurchaseOrderId)
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
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);

                _logger.LogInformation(
                    "✅ PO found: {PONumber} | DocumentStatus: {DocumentStatus} | ReceptionStatus: {ReceptionStatus}",
                    po.PONumber, po.DocumentStatus, po.ReceptionStatus);

                // ✅ Validate PO Status
                if (po.DocumentStatus  == Common.Enum.Purchases_Enum.DocumentStatus.Draft)
                    throw new BusinessLogicException(
                        "Cannot create GRN for a Purchase Order in Draft status.",
                        "Purchases",
                        FinanceErrorCode.ValidationError);

                if (po.DocumentStatus == Common.Enum.Purchases_Enum.DocumentStatus.Cancelled || po.DocumentStatus == Common.Enum.Purchases_Enum.DocumentStatus.Closed)
                    throw new BusinessLogicException(
                        $"Cannot create GRN for a Purchase Order in {po.DocumentStatus} status.",
                        "Purchases",
                        FinanceErrorCode.ValidationError);

                string previousReceptionStatus = po.ReceptionStatus.ToString();

                // ✅ 2. Load PO Line Items with Product Info
                var poLineItemIds = request.LineItems.Select(l => l.POLineItemId).ToList();
                var poLineItemsData = await _poLineItemRepository.GetAll()
                    .Where(x => poLineItemIds.Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        x.PurchaseOrderId,
                        x.Quantity,
                        x.ReceivedQuantity,
                        x.ProductId,
                        x.ServiceId,
                        x.Description,
                        x.UnitPrice,
                        ProductName = x.Product != null ? x.Product.Name : null,
                        ProductSKU = x.Product != null ? x.Product.SKU : null,
                        ServiceName = x.Service != null ? x.Service.Name : null
                    })
                    .ToListAsync(cancellationToken);

                // Validate all line items exist
                var foundIds = poLineItemsData.Select(x => x.Id).ToList();
                var missingIds = poLineItemIds.Except(foundIds).ToList();

                if (missingIds.Any())
                {
                    var missingIdsStr = string.Join(", ", missingIds);
                    throw new NotFoundException(
                        $"PO Line Item(s) not found: {missingIdsStr}",
                        FinanceErrorCode.NotFound);
                }

                // Validate all belong to same PO
                var invalidLineItems = poLineItemsData
                    .Where(x => x.PurchaseOrderId != request.PurchaseOrderId)
                    .ToList();

                if (invalidLineItems.Any())
                {
                    var invalidIds = string.Join(", ", invalidLineItems.Select(x => x.Id));
                    throw new BusinessLogicException(
                        $"Line items {invalidIds} do not belong to Purchase Order {request.PurchaseOrderId}",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                // ✅ 3. Validate Quantities
                foreach (var lineItem in request.LineItems)
                {
                    var poLineItem = poLineItemsData.FirstOrDefault(x => x.Id == lineItem.POLineItemId);
                    if (poLineItem == null)
                        throw new NotFoundException(
                            $"PO Line Item {lineItem.POLineItemId} not found",
                            FinanceErrorCode.NotFound);

                    decimal pendingQty = poLineItem.Quantity - poLineItem.ReceivedQuantity;
                    string itemName = poLineItem.ProductName ?? poLineItem.ServiceName ?? poLineItem.Description ?? "Unknown Item";

                    _logger.LogInformation(
                        "📊 Validating | POLineItemId: {POLineItemId} | Item: '{ItemName}' | " +
                        "Ordered: {Ordered} | Received: {Received} | Pending: {Pending} | Attempting: {Attempting}",
                        lineItem.POLineItemId, itemName, poLineItem.Quantity,
                        poLineItem.ReceivedQuantity, pendingQty, lineItem.ReceivedQuantity);

                    if (lineItem.ReceivedQuantity <= 0)
                        throw new BusinessLogicException(
                            $"Received quantity for '{itemName}' must be greater than zero.",
                            "Purchases",
                            FinanceErrorCode.ValidationError);

                    if (pendingQty <= 0)
                        throw new BusinessLogicException(
                            $"Cannot receive '{itemName}'. Already fully received.",
                            "Purchases",
                            FinanceErrorCode.ValidationError);

                    if (lineItem.ReceivedQuantity > pendingQty)
                        throw new BusinessLogicException(
                            $"Cannot receive {lineItem.ReceivedQuantity} units of '{itemName}'. " +
                            $"Only {pendingQty} units pending.",
                            "Purchases",
                            FinanceErrorCode.ValidationError);
                }

                // ✅ 4. Generate GRN Number
                var grnNumber = await GenerateGRNNumber(request.CompanyId, cancellationToken);
                _logger.LogInformation("✅ Generated GRN Number: {GRNNumber}", grnNumber);

                // ✅ 5. Create GRN
                var grn = new GoodsReceiptNote
                {
                    Id = Guid.NewGuid(),
                    GRNNumber = grnNumber,
                    PurchaseOrderId = request.PurchaseOrderId,
                    WarehouseId = request.WarehouseId,
                    CompanyId = request.CompanyId,
                    ReceiptDate = request.ReceiptDate,
                    ReceivedBy = request.ReceivedBy,
                    Notes = request.Notes,
                    CreatedById = request.UserId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                grn.LineItems = request.LineItems.Select(item => new GRNLineItem
                {
                    Id = Guid.NewGuid(),
                    GRNId = grn.Id,
                    POLineItemId = item.POLineItemId,
                    ReceivedQuantity = item.ReceivedQuantity,
                    Notes = item.Notes,
                    CreatedById = request.UserId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                }).ToList();

                await _grnRepository.AddAsync(grn);
                await _grnRepository.SaveChanges();

                _logger.LogInformation("✅ GRN entity created with Id: {GRNId}", grn.Id);

                // ✅ 6. Update PO Line Items ReceivedQuantity & RemainingQuantity
                foreach (var lineItem in request.LineItems)
                {
                    var poLineItem = await _poLineItemRepository.GetByIDWithTracking(lineItem.POLineItemId);
                    if (poLineItem != null)
                    {
                        decimal previousReceived = poLineItem.ReceivedQuantity;
                        poLineItem.ReceivedQuantity += lineItem.ReceivedQuantity;
                        poLineItem.RemainingQuantity = poLineItem.Quantity - poLineItem.ReceivedQuantity;
                        poLineItem.UpdatedAt = DateTime.UtcNow;
                        poLineItem.UpdatedById = request.UserId;

                        await _poLineItemRepository.SaveChanges();

                        _logger.LogInformation(
                            "✅ Updated PO Line Item | POLineItemId: {POLineItemId} | " +
                            "Previous: {Previous} → New: {New} | Increment: +{Increment}",
                            lineItem.POLineItemId, previousReceived,
                            poLineItem.ReceivedQuantity, lineItem.ReceivedQuantity);
                    }
                }

                // ✅ 7. Update Warehouse Stock
                _logger.LogInformation("🔄 Starting Warehouse Stock update...");

                foreach (var lineItem in request.LineItems)
                {
                    var poLineItem = poLineItemsData.FirstOrDefault(x => x.Id == lineItem.POLineItemId);

                    // Skip services (only update stock for products)
                    if (poLineItem?.ProductId == null)
                    {
                        _logger.LogInformation(
                            "⏭️ Skipping stock update for Service: '{ServiceName}' (not a product)",
                            poLineItem?.ServiceName ?? "Unknown Service");
                        continue;
                    }

                    var productId = poLineItem.ProductId.Value;
                    var warehouseId = request.WarehouseId;

                    // Find existing stock record
                    var existingStock = await _warehouseStockRepository.GetAll()
                        .Where(x => x.WarehouseId == warehouseId && x.ProductId == productId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (existingStock != null)
                    {
                        // Update existing stock
                        decimal previousQty = existingStock.Quantity;
                        decimal previousAvailable = existingStock.AvailableQuantity;

                        existingStock.Quantity += lineItem.ReceivedQuantity;
                        existingStock.AvailableQuantity += lineItem.ReceivedQuantity;
                        existingStock.LastStockInDate = DateTime.UtcNow;
                        existingStock.UpdatedAt = DateTime.UtcNow;
                        existingStock.UpdatedById = request.UserId;
                        existingStock.TotalValue = existingStock.Quantity * (existingStock.AverageUnitCost ?? poLineItem.UnitPrice);

                        await _warehouseStockRepository.SaveChanges();

                        _logger.LogInformation(
                            "✅ Stock Updated | ProductId: {ProductId} | Product: '{ProductName}' | " +
                            "Warehouse: {WarehouseId} | Qty: {PreviousQty} → {NewQty} (+{Added}) | " +
                            "Available: {PreviousAvailable} → {NewAvailable}",
                            productId, poLineItem.ProductName, warehouseId,
                            previousQty, existingStock.Quantity, lineItem.ReceivedQuantity,
                            previousAvailable, existingStock.AvailableQuantity);
                    }
                    else
                    {
                        // Create new stock record
                        var newStock = new WarehouseStock
                        {
                            WarehouseId = warehouseId,
                            ProductId = productId,
                            Quantity = lineItem.ReceivedQuantity,
                            AvailableQuantity = lineItem.ReceivedQuantity,
                            ReservedQuantity = 0,
                            AverageUnitCost = poLineItem.UnitPrice,
                            TotalValue = poLineItem.UnitPrice * lineItem.ReceivedQuantity,
                            LastStockInDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            CreatedById = request.UserId,
                            IsActive = true,
                            IsDeleted = false
                        };

                        await _warehouseStockRepository.AddAsync(newStock);
                        await _warehouseStockRepository.SaveChanges();

                        _logger.LogInformation(
                            "✅ New Stock Created | ProductId: {ProductId} | Product: '{ProductName}' | " +
                            "Warehouse: {WarehouseId} | Initial Qty: {Qty}",
                            productId, poLineItem.ProductName, warehouseId, lineItem.ReceivedQuantity);
                    }
                }

                _logger.LogInformation("✅ Warehouse Stock update completed successfully");

                // ✅ 8. Update PO Reception Status
                var allPoLineItems = await _poLineItemRepository.GetAll()
                    .Where(x => x.PurchaseOrderId == request.PurchaseOrderId)
                    .ToListAsync(cancellationToken);

                string newReceptionStatus = CalculateReceptionStatus(allPoLineItems);

                _logger.LogInformation(
                    "📊 PO Status Update | PO: {PONumber} | Previous: '{Previous}' → New: '{New}'",
                    po.PONumber, previousReceptionStatus, newReceptionStatus);

                var purchaseOrder = await _poRepository.GetByIDWithTracking(request.PurchaseOrderId);
                if (purchaseOrder != null)
                {
                    purchaseOrder.ReceptionStatus =
                        Enum.Parse<ReceptionStatus>(newReceptionStatus);
                    purchaseOrder.UpdatedAt = DateTime.UtcNow;
                    purchaseOrder.UpdatedById = request.UserId;
                    await _poRepository.SaveChanges();
                }

                // ✅ Commit Transaction
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "✅✅✅ GRN created successfully | GRN: {GRNNumber} | PO: {PONumber}",
                    grnNumber, po.PONumber);

                // ✅ 9. Build Complete Response
                var response = await BuildGRNResponse(
                    grn.Id,
                    previousReceptionStatus,
                    newReceptionStatus,
                    po.PaymentStatus.ToString(),
                    po.DocumentStatus.ToString(),
                    cancellationToken);

                return response;
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "❌ Error creating GRN for PO {PurchaseOrderId}. Transaction rolled back.",
                    request.PurchaseOrderId);
                throw new BusinessLogicException(
                    $"Error creating GRN: {ex.Message}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task<GRNResponseDto> BuildGRNResponse(
            Guid grnId,
            string previousReceptionStatus,
            string newReceptionStatus,
            string paymentStatus,
            string documentStatus,
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
                    LineItems = g.LineItems.Select(li => new
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
                throw new NotFoundException("GRN not found after creation", FinanceErrorCode.NotFound);

            decimal totalRemaining = 0;
            var lineItemsResponse = new List<GRNLineItemResponseDto>();
            var inventoryImpact = new List<InventoryImpactDto>();

            foreach (var li in grn.LineItems)
            {
                decimal previousReceived = li.ReceivedQty - li.CurrentReceived;
                decimal remaining = li.OrderedQty - li.ReceivedQty;
                totalRemaining += remaining;

                string displayName = (li.ProductName ?? li.ServiceName ?? li.Description ?? "Unknown Item").Trim();
                string displaySKU = (li.ProductSKU ?? "").Trim();

                if (string.IsNullOrWhiteSpace(displayName))
                    displayName = "Unknown Item";

                lineItemsResponse.Add(new GRNLineItemResponseDto
                {
                    Id = li.Id,
                    GRNId = grnId,
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

            if (paymentStatus != "Paid in Full" && paymentStatus != "Refunded")
                warnings.Add($"Payment status: '{paymentStatus}'. Full payment required before closing PO.");

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
                    ReceptionStatus = newReceptionStatus,
                    PaymentStatus = paymentStatus,
                    DocumentStatus = documentStatus,
                    PreviousReceptionStatus = previousReceptionStatus
                },
                LineItems = lineItemsResponse,
                InventoryImpact = inventoryImpact,
                NextActions = new GRNNextActionsDto
                {
                    CanReceiveMore = totalRemaining > 0,
                    CanCreateReturn = true,
                    CanCreateInvoice = true,
                    CanClose = newReceptionStatus == "Fully Received" && paymentStatus == "Paid in Full",
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

        private async Task<string> GenerateGRNNumber(Guid companyId, CancellationToken cancellationToken)
        {
            var year = DateTime.UtcNow.Year;
            var lastGrn = await _grnRepository.GetByCompanyId(companyId)
                .Where(x => x.GRNNumber.StartsWith($"GRN-{year}-"))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.GRNNumber)
                .FirstOrDefaultAsync(cancellationToken);

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastGrn))
            {
                var parts = lastGrn.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                    nextNumber = lastNumber + 1;
            }

            return $"GRN-{year}-{nextNumber:D5}";
        }
    }
}
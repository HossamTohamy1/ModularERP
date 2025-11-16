using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
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
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateGRNHandler> _logger;

        public UpdateGRNHandler(
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<GRNLineItem> lineItemRepository,
            IGeneralRepository<POLineItem> poLineItemRepository,
            IMapper mapper,
            ILogger<UpdateGRNHandler> logger)
        {
            _poRepository = poRepository;
            _grnRepository = grnRepository;
            _lineItemRepository = lineItemRepository;
            _poLineItemRepository = poLineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<GRNResponseDto>> Handle(UpdateGRNCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating GRN with ID: {GRNId}", request.Data.Id);

                // ✅ Get existing GRN
                var existingGrn = await _grnRepository.GetByIDWithTracking(request.Data.Id);
                if (existingGrn == null)
                {
                    throw new NotFoundException(
                        $"GRN with ID {request.Data.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Existing GRN found: {GRNNumber} | PO: {POId}",
                    existingGrn.GRNNumber, existingGrn.PurchaseOrderId);

                // ✅ Get current PO status for tracking
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
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {existingGrn.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                string previousReceptionStatus = po.ReceptionStatus;

                // ✅ Get existing line items with their quantities
                var existingLineItems = await _lineItemRepository.GetAll()
                    .Where(x => x.GRNId == request.Data.Id && !x.IsDeleted)
                    .Select(x => new
                    {
                        x.Id,
                        x.POLineItemId,
                        x.ReceivedQuantity
                    })
                    .ToListAsync(cancellationToken);

                // ✅ Calculate quantity changes to update POLineItems
                var quantityChanges = new Dictionary<Guid, decimal>(); // POLineItemId -> Delta

                // Calculate removals (deleted line items)
                var lineItemIdsToKeep = request.Data.LineItems
                    .Where(x => x.Id.HasValue)
                    .Select(x => x.Id!.Value)
                    .ToList();

                var lineItemsToDelete = existingLineItems
                    .Where(x => !lineItemIdsToKeep.Contains(x.Id))
                    .ToList();

                foreach (var deletedItem in lineItemsToDelete)
                {
                    // Subtract the quantity that was previously received
                    if (!quantityChanges.ContainsKey(deletedItem.POLineItemId))
                        quantityChanges[deletedItem.POLineItemId] = 0;

                    quantityChanges[deletedItem.POLineItemId] -= deletedItem.ReceivedQuantity;

                    _logger.LogInformation("Line item deleted | GRNLineItemId: {Id} | POLineItemId: {POLineItemId} | Quantity to subtract: {Qty}",
                        deletedItem.Id, deletedItem.POLineItemId, deletedItem.ReceivedQuantity);
                }

                // Calculate updates and additions
                foreach (var lineItemDto in request.Data.LineItems)
                {
                    if (!quantityChanges.ContainsKey(lineItemDto.POLineItemId))
                        quantityChanges[lineItemDto.POLineItemId] = 0;

                    if (lineItemDto.Id.HasValue)
                    {
                        // Updated line item - calculate delta
                        var existing = existingLineItems.FirstOrDefault(x => x.Id == lineItemDto.Id.Value);
                        if (existing != null)
                        {
                            decimal delta = lineItemDto.ReceivedQuantity - existing.ReceivedQuantity;
                            quantityChanges[lineItemDto.POLineItemId] += delta;

                            _logger.LogInformation("Line item updated | GRNLineItemId: {Id} | POLineItemId: {POLineItemId} | Old: {Old} | New: {New} | Delta: {Delta}",
                                lineItemDto.Id.Value, lineItemDto.POLineItemId, existing.ReceivedQuantity, lineItemDto.ReceivedQuantity, delta);
                        }
                    }
                    else
                    {
                        // New line item - add full quantity
                        quantityChanges[lineItemDto.POLineItemId] += lineItemDto.ReceivedQuantity;

                        _logger.LogInformation("Line item added | POLineItemId: {POLineItemId} | Quantity to add: {Qty}",
                            lineItemDto.POLineItemId, lineItemDto.ReceivedQuantity);
                    }
                }

                // ✅ Validate quantities before applying changes
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
                    {
                        throw new NotFoundException(
                            $"PO Line Item {change.Key} not found",
                            FinanceErrorCode.NotFound);
                    }

                    decimal newReceivedQty = poLineItem.ReceivedQuantity + change.Value;

                    if (newReceivedQty < 0)
                    {
                        string itemName = poLineItem.ProductName ?? poLineItem.ServiceName ?? poLineItem.Description ?? "Unknown Item";
                        itemName = string.IsNullOrWhiteSpace(itemName) ? "Unknown Item" : itemName.Trim();

                        throw new BusinessLogicException(
                            $"Cannot update GRN. The change would result in negative received quantity for '{itemName}'.",
                            module: "Purchases",
                            financeErrorCode: FinanceErrorCode.ValidationError
                        );
                    }

                    if (newReceivedQty > poLineItem.Quantity)
                    {
                        string itemName = poLineItem.ProductName ?? poLineItem.ServiceName ?? poLineItem.Description ?? "Unknown Item";
                        itemName = string.IsNullOrWhiteSpace(itemName) ? "Unknown Item" : itemName.Trim();

                        throw new BusinessLogicException(
                            $"Cannot update GRN. Total received quantity ({newReceivedQty}) would exceed ordered quantity ({poLineItem.Quantity}) for '{itemName}'.",
                            module: "Purchases",
                            financeErrorCode: FinanceErrorCode.ValidationError
                        );
                    }
                }

                // ✅ Apply quantity changes to POLineItems
                foreach (var change in quantityChanges)
                {
                    if (change.Value != 0) // Only update if there's an actual change
                    {
                        var poLineItem = await _poLineItemRepository.GetByIDWithTracking(change.Key);
                        if (poLineItem != null)
                        {
                            decimal oldQty = poLineItem.ReceivedQuantity;
                            poLineItem.ReceivedQuantity += change.Value;
                            await _poLineItemRepository.SaveChanges();

                            _logger.LogInformation("POLineItem updated | Id: {Id} | Old Received: {Old} | Change: {Change} | New Received: {New}",
                                change.Key, oldQty, change.Value, poLineItem.ReceivedQuantity);
                        }
                    }
                }

                // ✅ Update PO Reception Status (CRITICAL FIX)
                var allPoLineItems = await _poLineItemRepository.GetAll()
                    .Where(x => x.PurchaseOrderId == existingGrn.PurchaseOrderId)
                    .ToListAsync(cancellationToken);

                string newReceptionStatus = CalculateReceptionStatus(allPoLineItems);

                if (newReceptionStatus != previousReceptionStatus)
                {
                    var purchaseOrder = await _poRepository.GetByIDWithTracking(existingGrn.PurchaseOrderId);
                    if (purchaseOrder != null)
                    {
                        purchaseOrder.ReceptionStatus = newReceptionStatus;
                        await _poRepository.SaveChanges();

                        _logger.LogInformation(
                            "✅ PO Reception Status updated | PO: {PONumber} | Previous: {Previous} | New: {New}",
                            po.PONumber, previousReceptionStatus, newReceptionStatus);
                    }
                }
                else
                {
                    _logger.LogInformation(
                        "PO Reception Status unchanged | PO: {PONumber} | Status: {Status}",
                        po.PONumber, newReceptionStatus);
                }

                // ✅ Update GRN main properties
                existingGrn.WarehouseId = request.Data.WarehouseId;
                existingGrn.ReceiptDate = request.Data.ReceiptDate;
                existingGrn.ReceivedBy = request.Data.ReceivedBy;
                existingGrn.Notes = request.Data.Notes;
                existingGrn.UpdatedAt = DateTime.UtcNow;

                // ✅ Delete removed line items
                foreach (var lineItem in lineItemsToDelete)
                {
                    await _lineItemRepository.Delete(lineItem.Id);
                }

                // ✅ Update or add line items
                foreach (var lineItemDto in request.Data.LineItems)
                {
                    if (lineItemDto.Id.HasValue)
                    {
                        // Update existing line item
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
                        // Add new line item
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

                _logger.LogInformation("✅ GRN updated successfully: {GRNNumber}", existingGrn.GRNNumber);

                // ✅ Build complete response with updated PO status
                var response = await BuildGRNResponse(
                    request.Data.Id,
                    previousReceptionStatus,
                    newReceptionStatus,
                    cancellationToken);

                return ResponseViewModel<GRNResponseDto>.Success(
                    response,
                    "GRN updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating GRN with ID: {GRNId}", request.Data.Id);
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
            {
                throw new NotFoundException("GRN not found after update", FinanceErrorCode.NotFound);
            }

            decimal totalRemaining = 0;
            var lineItemsResponse = new List<GRNLineItemResponseDto>();
            var inventoryImpact = new List<InventoryImpactDto>();

            foreach (var li in grn.LineItems)
            {
                decimal previousReceived = li.ReceivedQty - li.CurrentReceived;
                decimal remaining = li.OrderedQty - li.ReceivedQty;
                totalRemaining += remaining;

                // Get display name with proper fallback
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

                // Only add inventory impact for Products
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
            {
                warnings.Add($"{totalRemaining} units still pending receipt for this PO");
            }

            if (grn.POPaymentStatus != "Paid in Full" && grn.POPaymentStatus != "Refunded")
            {
                warnings.Add($"Payment status is '{grn.POPaymentStatus}'. Full payment required before closing PO.");
            }

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
                    PaymentStatus = grn.POPaymentStatus ?? "Unknown",
                    DocumentStatus = grn.PODocumentStatus ?? "Unknown",
                    PreviousReceptionStatus = previousReceptionStatus
                },
                LineItems = lineItemsResponse,
                InventoryImpact = inventoryImpact,
                NextActions = new GRNNextActionsDto
                {
                    CanReceiveMore = totalRemaining > 0,
                    CanCreateReturn = true,
                    CanCreateInvoice = true,
                    CanClose = currentReceptionStatus == "Fully Received" && grn.POPaymentStatus == "Paid in Full",
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
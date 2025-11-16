using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRN
{
    public class ReverseGRNHandler : IRequestHandler<ReverseGRNCommand, GRNResponseDto>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineItemRepository;
        private readonly IGeneralRepository<POLineItem> _poLineItemRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ReverseGRNHandler> _logger;

        public ReverseGRNHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<GRNLineItem> grnLineItemRepository,
            IGeneralRepository<POLineItem> poLineItemRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IMapper mapper,
            ILogger<ReverseGRNHandler> logger)
        {
            _grnRepository = grnRepository;
            _grnLineItemRepository = grnLineItemRepository;
            _poLineItemRepository = poLineItemRepository;
            _poRepository = poRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GRNResponseDto> Handle(ReverseGRNCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Reversing GRN {GRNId}", request.GRNId);

                // ✅ Get GRN with tracking for updates
                var grn = await _grnRepository.GetByIDWithTracking(request.GRNId);
                if (grn == null)
                {
                    throw new NotFoundException(
                        $"GRN with ID {request.GRNId} not found",
                        FinanceErrorCode.NotFound);
                }

                // ✅ Get line items - check both active and deleted
                var allLineItems = await _grnLineItemRepository
                    .GetAll()
                    .Where(l => l.GRNId == request.GRNId)
                    .ToListAsync(cancellationToken);

                if (allLineItems.Count == 0)
                {
                    throw new BusinessLogicException(
                        "Cannot reverse GRN without line items",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                // ✅ Check if already reversed
                if (allLineItems.All(li => li.IsDeleted))
                {
                    throw new BusinessLogicException(
                        "This GRN has already been reversed",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                // Only process active line items
                var lineItems = allLineItems.Where(li => !li.IsDeleted).ToList();

                if (lineItems.Count == 0)
                {
                    throw new BusinessLogicException(
                        "This GRN has already been reversed",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                // ✅ Reverse inventory quantities
                foreach (var lineItem in lineItems)
                {
                    var poLineItem = await _poLineItemRepository.GetByIDWithTracking(lineItem.POLineItemId);
                    if (poLineItem != null)
                    {
                        // Decrease ReceivedQuantity by the amount in this GRN
                        poLineItem.ReceivedQuantity -= lineItem.ReceivedQuantity;

                        // Ensure it doesn't go negative
                        if (poLineItem.ReceivedQuantity < 0)
                        {
                            poLineItem.ReceivedQuantity = 0;
                        }
                    }
                }

                // ✅ Update PO Reception Status
                var purchaseOrder = await _poRepository.GetByIDWithTracking(grn.PurchaseOrderId);
                if (purchaseOrder != null)
                {
                    var allPoLineItems = await _poLineItemRepository
                        .GetAll()
                        .Where(li => li.PurchaseOrderId == purchaseOrder.Id && !li.IsDeleted)
                        .ToListAsync(cancellationToken);

                    var totalOrdered = allPoLineItems.Sum(li => li.Quantity);
                    var totalReceived = allPoLineItems.Sum(li => li.ReceivedQuantity);

                    // Update Reception Status based on quantities
                    if (totalReceived == 0)
                    {
                        purchaseOrder.ReceptionStatus = "Not Received";
                    }
                    else if (totalReceived < totalOrdered)
                    {
                        purchaseOrder.ReceptionStatus = "Partially Received";
                    }
                    else
                    {
                        purchaseOrder.ReceptionStatus = "Fully Received";
                    }
                }

                // ✅ Update GRN notes - Prevent duplication
                string reversalNote = $"Reversed: {request.ReversalReason}";

                // Only add if not already present
                if (string.IsNullOrEmpty(grn.Notes))
                {
                    grn.Notes = reversalNote;
                }
                else if (!grn.Notes.Contains(reversalNote))
                {
                    grn.Notes = $"{grn.Notes}\n\n{reversalNote}";
                }
                else
                {
                    _logger.LogWarning("Reversal note already exists in GRN {GRNId}, skipping duplicate", request.GRNId);
                }

                grn.UpdatedById = request.UserId;
                grn.UpdatedAt = DateTime.UtcNow;

                // ✅ Soft delete all line items
                foreach (var lineItem in lineItems)
                {
                    lineItem.IsDeleted = true;

                }

                // Save all changes
                await _grnRepository.SaveChanges();
                await _grnLineItemRepository.SaveChanges();
                await _poLineItemRepository.SaveChanges();
                await _poRepository.SaveChanges();

                _logger.LogInformation("✅ GRN {GRNId} reversed successfully", request.GRNId);

                // ✅ Build complete response showing reversed items
                var response = await BuildGRNReversalResponse(request.GRNId, cancellationToken);

                return response;
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                _logger.LogError(ex, "❌ Error reversing GRN {GRNId}", request.GRNId);
                throw new BusinessLogicException(
                    $"Error reversing GRN: {ex.Message}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }

        private async Task<GRNResponseDto> BuildGRNReversalResponse(Guid grnId, CancellationToken cancellationToken)
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
                    // Get ALL line items including deleted ones to show what was reversed
                    AllLineItems = g.LineItems
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
                            ReversedQty = li.ReceivedQuantity, // The quantity that was reversed
                            li.Notes,
                            li.IsDeleted
                        }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (grn == null)
            {
                throw new NotFoundException("GRN not found", FinanceErrorCode.NotFound);
            }

            decimal totalRemaining = 0;
            var lineItemsResponse = new List<GRNLineItemResponseDto>();
            var inventoryImpact = new List<InventoryImpactDto>();

            foreach (var li in grn.AllLineItems)
            {
                // Calculate remaining based on current ReceivedQty (after reversal)
                decimal remaining = li.OrderedQty - li.ReceivedQty;
                totalRemaining += remaining;

                // Get display name with proper fallback
                string displayName = li.ProductName ?? li.ServiceName ?? li.Description ?? "Unknown Item";
                displayName = string.IsNullOrWhiteSpace(displayName) ? "Unknown Item" : displayName.Trim();

                string displaySKU = string.IsNullOrWhiteSpace(li.ProductSKU) ? "" : li.ProductSKU.Trim();

                // Show reversed items with their original quantities
                lineItemsResponse.Add(new GRNLineItemResponseDto
                {
                    Id = li.Id,
                    POLineItemId = li.POLineItemId,
                    ProductId = li.ProductId,
                    ProductName = displayName + (li.IsDeleted ? " [REVERSED]" : ""),
                    ProductSKU = displaySKU,
                    OrderedQuantity = li.OrderedQty,
                    PreviouslyReceivedQuantity = li.ReceivedQty + li.ReversedQty, // Before reversal
                    ReceivedQuantity = li.ReversedQty, // What was in this GRN
                    RemainingQuantity = remaining, // After reversal
                    IsFullyReceived = remaining == 0,
                    UnitOfMeasure = null,
                    Notes = li.Notes
                });

                // Show inventory impact with NEGATIVE quantities (reversal)
                if (li.ProductId.HasValue && li.IsDeleted)
                {
                    inventoryImpact.Add(new InventoryImpactDto
                    {
                        ProductId = li.ProductId.Value,
                        ProductName = displayName,
                        ProductSKU = displaySKU,
                        WarehouseId = grn.WarehouseId,
                        WarehouseName = grn.WarehouseName,
                        QuantityAdded = -li.ReversedQty, // NEGATIVE to show reversal
                        PreviousStock = 0, // TODO: Fetch from Inventory module
                        NewStock = 0 // TODO: Fetch from Inventory module
                    });
                }
            }

            var warnings = new List<string>();
            warnings.Add("⚠️ This GRN has been reversed. All quantities have been deducted from inventory and PO received totals.");

            if (totalRemaining > 0)
            {
                warnings.Add($"📦 {totalRemaining} units now pending receipt for PO {grn.PONumber}");
            }

            // Check if PO can be closed per BRSD rules:
            // "A PO cannot move to Closed unless: Reception Status = Fully Received, Payment Status = Paid in Full"
            bool canClose = grn.POReceptionStatus == "Fully Received" &&
                           grn.POPaymentStatus == "Paid in Full";

            // Per BRSD: "Partially Received → Allow more receipts/returns/payments"
            // Invoice can be created as long as SOMETHING has been received (not "Not Received")
            bool canCreateInvoice = grn.POReceptionStatus != "Not Received";

            // Add warning if trying to invoice before full receipt
            if (canCreateInvoice && grn.POReceptionStatus == "Partially Received")
            {
                warnings.Add("💡 You can create an invoice for partially received items, but some quantities are still pending.");
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
                    ReceptionStatus = grn.POReceptionStatus ?? "Unknown",
                    PaymentStatus = grn.POPaymentStatus ?? "Unknown",
                    DocumentStatus = grn.PODocumentStatus ?? "Unknown",
                    PreviousReceptionStatus = null // Can track this if needed
                },
                LineItems = lineItemsResponse, // Show reversed items
                InventoryImpact = inventoryImpact, // Show negative impact
                NextActions = new GRNNextActionsDto
                {
                    CanReceiveMore = totalRemaining > 0, // ✅ True if there's remaining qty
                    CanCreateReturn = false, // Can't return an already reversed GRN
                    CanCreateInvoice = canCreateInvoice, // ✅ Based on reception status
                    CanClose = canClose, // ✅ Per BRSD rules
                    TotalRemainingToReceive = totalRemaining, // ✅ Show actual remaining
                    Warnings = warnings
                }
            };
        }
    }
}
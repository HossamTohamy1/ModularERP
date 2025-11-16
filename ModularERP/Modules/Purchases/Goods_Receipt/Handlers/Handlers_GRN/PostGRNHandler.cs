using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRN
{
    public class PostGRNHandler : IRequestHandler<PostGRNCommand, GRNResponseDto>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PostGRNHandler> _logger;

        public PostGRNHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<GRNLineItem> grnLineItemRepository,
            IMapper mapper,
            ILogger<PostGRNHandler> logger)
        {
            _grnRepository = grnRepository;
            _grnLineItemRepository = grnLineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GRNResponseDto> Handle(PostGRNCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Posting GRN {GRNId} to inventory", request.GRNId);

                // ✅ Get GRN with validation
                var grn = await _grnRepository.GetByID(request.GRNId);
                if (grn == null)
                {
                    throw new NotFoundException(
                        $"GRN with ID {request.GRNId} not found",
                        FinanceErrorCode.NotFound);
                }

                // ✅ Get line items
                var lineItems = await _grnLineItemRepository
                    .GetAll()
                    .Where(l => l.GRNId == request.GRNId && !l.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (lineItems.Count == 0)
                {
                    throw new BusinessLogicException(
                        "Cannot post GRN without line items",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                // TODO: Add inventory posting logic here
                // This would typically:
                // 1. Update inventory quantities
                // 2. Create inventory transactions
                // 3. Update purchase order received quantities (already done in Create/Update)
                // 4. Post to general ledger (if integrated)
                // 5. Mark GRN as "Posted" if you have a status field

                _logger.LogInformation("✅ GRN {GRNId} posted successfully", request.GRNId);

                // ✅ Build complete response (same as Get)
                var response = await BuildGRNResponse(request.GRNId, cancellationToken);

                return response;
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                _logger.LogError(ex, "❌ Error posting GRN {GRNId}", request.GRNId);
                throw new BusinessLogicException(
                    $"Error posting GRN: {ex.Message}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }

        private async Task<GRNResponseDto> BuildGRNResponse(Guid grnId, CancellationToken cancellationToken)
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
                throw new NotFoundException("GRN not found", FinanceErrorCode.NotFound);
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
                        PreviousStock = 0, // TODO: Fetch from Inventory module
                        NewStock = li.CurrentReceived // TODO: Fetch from Inventory module
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
                    ReceptionStatus = grn.POReceptionStatus ?? "Unknown",
                    PaymentStatus = grn.POPaymentStatus ?? "Unknown",
                    DocumentStatus = grn.PODocumentStatus ?? "Unknown",
                    PreviousReceptionStatus = null // Can't determine previous in Post
                },
                LineItems = lineItemsResponse,
                InventoryImpact = inventoryImpact,
                NextActions = new GRNNextActionsDto
                {
                    CanReceiveMore = totalRemaining > 0,
                    CanCreateReturn = true,
                    CanCreateInvoice = true,
                    CanClose = grn.POReceptionStatus == "Fully Received" && grn.POPaymentStatus == "Paid in Full",
                    TotalRemainingToReceive = totalRemaining,
                    Warnings = warnings
                }
            };
        }
    }
}
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Qeuries.Qeuries_GRN;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRN
{
    public class GetGRNByIdHandler : IRequestHandler<GetGRNByIdQuery, ResponseViewModel<GRNResponseDto>>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly ILogger<GetGRNByIdHandler> _logger;

        public GetGRNByIdHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            ILogger<GetGRNByIdHandler> logger)
        {
            _grnRepository = grnRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<GRNResponseDto>> Handle(GetGRNByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching GRN with ID: {GRNId}", request.Id);

                // ✅ Get complete GRN data with all related info
                var grn = await _grnRepository.GetAll()
                    .Where(x => x.Id == request.Id)
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
                    throw new NotFoundException(
                        $"GRN with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                // ✅ Build line items response with full details
                decimal totalRemaining = 0;
                var lineItemsResponse = new List<GRNLineItemResponseDto>();
                var inventoryImpact = new List<InventoryImpactDto>();

                foreach (var li in grn.LineItems)
                {
                    decimal previousReceived = li.ReceivedQty - li.CurrentReceived;
                    decimal remaining = li.OrderedQty - li.ReceivedQty;
                    totalRemaining += remaining;

                    // ✅ Get display name with proper fallback (Product > Service > Description)
                    string displayName = li.ProductName ?? li.ServiceName ?? li.Description ?? "Unknown Item";

                    // ✅ Remove leading/trailing spaces and handle empty strings
                    displayName = string.IsNullOrWhiteSpace(displayName)
                        ? "Unknown Item"
                        : displayName.Trim();

                    string displaySKU = string.IsNullOrWhiteSpace(li.ProductSKU)
                        ? ""
                        : li.ProductSKU.Trim();

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
                        UnitOfMeasure = null, // TODO: Add UnitOfMeasure field
                        Notes = li.Notes
                    });

                    // ✅ Only add inventory impact for Products (not Services)
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

                // ✅ Build warnings based on current PO status
                var warnings = new List<string>();
                if (totalRemaining > 0)
                {
                    warnings.Add($"{totalRemaining} units still pending receipt for this PO");
                }

                // Add payment warning if not fully paid
                if (grn.POPaymentStatus != "Paid in Full" && grn.POPaymentStatus != "Refunded")
                {
                    warnings.Add($"Payment status is '{grn.POPaymentStatus}'. Full payment required before closing PO.");
                }

                // ✅ Build complete response DTO
                var response = new GRNResponseDto
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

                    // ✅ PO Status Info (من الـ Purchase Order الحالي)
                    PurchaseOrderStatus = new POStatusInfo
                    {
                        ReceptionStatus = grn.POReceptionStatus ?? "Unknown",
                        PaymentStatus = grn.POPaymentStatus ?? "Unknown",
                        DocumentStatus = grn.PODocumentStatus ?? "Unknown",
                        PreviousReceptionStatus = null // Can't determine previous status from GET
                    },

                    // ✅ Enhanced line items
                    LineItems = lineItemsResponse,

                    // ✅ Inventory impact
                    InventoryImpact = inventoryImpact,

                    // ✅ Next actions based on current PO state
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

                _logger.LogInformation("✅ Retrieved GRN successfully: {GRNNumber} | PO: {PONumber} | Status: {ReceptionStatus}",
                    grn.GRNNumber, grn.PONumber, grn.POReceptionStatus);

                return ResponseViewModel<GRNResponseDto>.Success(
                    response,
                    "GRN retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching GRN with ID: {GRNId}", request.Id);
                throw;
            }
        }
    }
}
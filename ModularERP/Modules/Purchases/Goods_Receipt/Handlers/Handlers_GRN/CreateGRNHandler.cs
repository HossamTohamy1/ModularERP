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
using Microsoft.AspNetCore.Http;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRN
{
    public class CreateGRNHandler : IRequestHandler<CreateGRNCommand, ResponseViewModel<GRNResponseDto>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<GRNLineItem> _lineItemRepository;
        private readonly IGeneralRepository<POLineItem> _poLineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateGRNHandler> _logger;

        public CreateGRNHandler(
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<GRNLineItem> lineItemRepository,
            IGeneralRepository<POLineItem> poLineItemRepository,
            IMapper mapper,
            ILogger<CreateGRNHandler> logger)
        {
            _poRepository = poRepository;
            _grnRepository = grnRepository;
            _lineItemRepository = lineItemRepository;
            _poLineItemRepository = poLineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<GRNResponseDto>> Handle(CreateGRNCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new GRN for PO: {PurchaseOrderId}", request.Data.PurchaseOrderId);

                // ✅ 1. Validate Purchase Order exists and get current status
                var po = await _poRepository.GetAll()
                    .Where(x => x.Id == request.Data.PurchaseOrderId)
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
                        $"Purchase Order with ID {request.Data.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation(
                    "PO found: {PONumber} | DocumentStatus: {DocumentStatus} | ReceptionStatus: {ReceptionStatus} | PaymentStatus: {PaymentStatus}",
                    po.PONumber, po.DocumentStatus, po.ReceptionStatus, po.PaymentStatus);

                // ✅ Validate PO is not in Draft or Cancelled
                if (po.DocumentStatus == "Draft")
                {
                    throw new BusinessLogicException(
                        "Cannot create GRN for a Purchase Order in Draft status. Please approve the PO first.",
                        module: "Purchases",
                        financeErrorCode: FinanceErrorCode.ValidationError
                    );
                }

                if (po.DocumentStatus == "Cancelled" || po.DocumentStatus == "Closed")
                {
                    throw new BusinessLogicException(
                        $"Cannot create GRN for a Purchase Order in {po.DocumentStatus} status.",
                        module: "Purchases",
                        financeErrorCode: FinanceErrorCode.ValidationError
                    );
                }

                string previousReceptionStatus = po.ReceptionStatus;

                // ✅ 2. Validate received quantities against PO line items
                var poLineItemsData = await _poLineItemRepository.GetAll()
                    .Where(x => request.Data.LineItems.Select(li => li.POLineItemId).Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        x.PurchaseOrderId,
                        x.Quantity,
                        x.ReceivedQuantity,
                        x.ProductId,
                        x.ServiceId,
                        x.Description,
                        ProductName = x.Product != null ? x.Product.Name : null,
                        ProductSKU = x.Product != null ? x.Product.SKU : null,
                        ServiceName = x.Service != null ? x.Service.Name : null
                    })
                    .ToListAsync(cancellationToken);

                // Validate all requested line items exist
                var requestedIds = request.Data.LineItems.Select(li => li.POLineItemId).ToList();
                var foundIds = poLineItemsData.Select(x => x.Id).ToList();
                var missingIds = requestedIds.Except(foundIds).ToList();

                if (missingIds.Any())
                {
                    var missingIdsStr = string.Join(", ", missingIds);
                    _logger.LogError("PO Line Item(s) not found: {MissingIds}", missingIdsStr);
                    throw new NotFoundException(
                        $"PO Line Item(s) not found: {missingIdsStr}",
                        FinanceErrorCode.NotFound);
                }

                // Validate all line items belong to the same PO
                var invalidLineItems = poLineItemsData
                    .Where(x => x.PurchaseOrderId != request.Data.PurchaseOrderId)
                    .ToList();

                if (invalidLineItems.Any())
                {
                    var invalidIds = string.Join(", ", invalidLineItems.Select(x => x.Id));
                    _logger.LogError(
                        "Line items {InvalidIds} do not belong to PO {PurchaseOrderId}",
                        invalidIds, request.Data.PurchaseOrderId);

                    throw new BusinessLogicException(
                        $"Line items {invalidIds} do not belong to Purchase Order {request.Data.PurchaseOrderId}",
                        module: "Purchases",
                        financeErrorCode: FinanceErrorCode.ValidationError
                    );
                }

                // Validate quantities for each line item
                foreach (var lineItem in request.Data.LineItems)
                {
                    var poLineItem = poLineItemsData.FirstOrDefault(x => x.Id == lineItem.POLineItemId);

                    if (poLineItem == null)
                    {
                        throw new NotFoundException(
                            $"PO Line Item {lineItem.POLineItemId} not found",
                            FinanceErrorCode.NotFound);
                    }

                    decimal pendingQty = poLineItem.Quantity - poLineItem.ReceivedQuantity;

                    // Get display name (Product > Service > Description)
                    string itemName = poLineItem.ProductName ?? poLineItem.ServiceName ?? poLineItem.Description ?? "Unknown Item";
                    string itemSKU = poLineItem.ProductSKU ?? "";

                    // ⚠️ Log detailed info for debugging
                    _logger.LogInformation(
                        "Validating line item | POLineItemId: {POLineItemId} | Item: '{ItemName}' ({ItemSKU}) | " +
                        "Ordered: {Ordered} | Previously Received: {Received} | Pending: {Pending} | Attempting to Receive: {Attempting}",
                        lineItem.POLineItemId,
                        itemName,
                        itemSKU,
                        poLineItem.Quantity,
                        poLineItem.ReceivedQuantity,
                        pendingQty,
                        lineItem.ReceivedQuantity
                    );

                    // Validate positive quantity
                    if (lineItem.ReceivedQuantity <= 0)
                    {
                        throw new BusinessLogicException(
                            $"Received quantity for '{itemName}' must be greater than zero. Provided: {lineItem.ReceivedQuantity}",
                            module: "Purchases",
                            financeErrorCode: FinanceErrorCode.ValidationError
                        );
                    }

                    // Check if already fully received
                    if (pendingQty <= 0)
                    {
                        _logger.LogError(
                            "Line item already fully received | POLineItemId: {POLineItemId} | Item: '{ItemName}' | " +
                            "Ordered: {Ordered} | Already Received: {Received}",
                            lineItem.POLineItemId, itemName, poLineItem.Quantity, poLineItem.ReceivedQuantity);

                        throw new BusinessLogicException(
                            $"Cannot receive '{itemName}'. This item has already been fully received. " +
                            $"Ordered: {poLineItem.Quantity}, Already Received: {poLineItem.ReceivedQuantity}, Pending: 0. " +
                            $"Please check if a previous GRN was created for this item.",
                            module: "Purchases",
                            financeErrorCode: FinanceErrorCode.ValidationError
                        );
                    }

                    // Check if attempting to over-receive
                    if (lineItem.ReceivedQuantity > pendingQty)
                    {
                        _logger.LogError(
                            "Attempting to over-receive | POLineItemId: {POLineItemId} | Item: '{ItemName}' | " +
                            "Ordered: {Ordered} | Already Received: {Received} | Pending: {Pending} | Attempting: {Attempting}",
                            lineItem.POLineItemId, itemName, poLineItem.Quantity, poLineItem.ReceivedQuantity,
                            pendingQty, lineItem.ReceivedQuantity);

                        throw new BusinessLogicException(
                            $"Cannot receive {lineItem.ReceivedQuantity} units of '{itemName}'. " +
                            $"Ordered: {poLineItem.Quantity}, Already Received: {poLineItem.ReceivedQuantity}, " +
                            $"Only {pendingQty} units pending.",
                            module: "Purchases",
                            financeErrorCode: FinanceErrorCode.ValidationError
                        );
                    }

                    _logger.LogInformation(
                        "✅ Validation passed for line item | POLineItemId: {POLineItemId} | Item: '{ItemName}' | Receiving: {ReceivingQty}",
                        lineItem.POLineItemId, itemName, lineItem.ReceivedQuantity);
                }

                // ✅ 3. Generate GRN Number
                var grnNumber = await GenerateGRNNumber(request.Data.CompanyId, cancellationToken);
                _logger.LogInformation("Generated GRN Number: {GRNNumber}", grnNumber);

                // ✅ 4. Create GRN
                var grn = _mapper.Map<GoodsReceiptNote>(request.Data);
                grn.GRNNumber = grnNumber;

                await _grnRepository.AddAsync(grn);
                await _grnRepository.SaveChanges();

                _logger.LogInformation("GRN entity created with Id: {GRNId}", grn.Id);

                // ✅ 5. Update PO Line Items ReceivedQuantity
                foreach (var lineItem in request.Data.LineItems)
                {
                    var poLineItem = await _poLineItemRepository.GetByIDWithTracking(lineItem.POLineItemId);
                    if (poLineItem != null)
                    {
                        decimal previousReceived = poLineItem.ReceivedQuantity;
                        poLineItem.ReceivedQuantity += lineItem.ReceivedQuantity;
                        await _poLineItemRepository.SaveChanges();

                        _logger.LogInformation(
                            "Updated PO Line Item | POLineItemId: {POLineItemId} | Previous Received: {PreviousReceived} | " +
                            "New Received: {NewReceived} | Increment: {Increment}",
                            lineItem.POLineItemId, previousReceived, poLineItem.ReceivedQuantity, lineItem.ReceivedQuantity);
                    }
                }

                // ✅ 6. Update PO Reception Status
                var allPoLineItems = await _poLineItemRepository.GetAll()
                    .Where(x => x.PurchaseOrderId == request.Data.PurchaseOrderId)
                    .ToListAsync(cancellationToken);

                string newReceptionStatus = CalculateReceptionStatus(allPoLineItems);

                _logger.LogInformation(
                    "Updating PO Reception Status | PO: {PONumber} | Previous: {PreviousStatus} | New: {NewStatus}",
                    po.PONumber, previousReceptionStatus, newReceptionStatus);

                var purchaseOrder = await _poRepository.GetByIDWithTracking(request.Data.PurchaseOrderId);
                if (purchaseOrder != null)
                {
                    purchaseOrder.ReceptionStatus = newReceptionStatus;
                    await _poRepository.SaveChanges();
                }

                _logger.LogInformation("✅ GRN created successfully | GRN Number: {GRNNumber} | PO: {PONumber}",
                    grnNumber, po.PONumber);

                // ✅ 7. Build Enhanced Response
                var createdGrn = await BuildGRNResponse(
                    grn.Id,
                    previousReceptionStatus,
                    newReceptionStatus,
                    po.PaymentStatus,
                    po.DocumentStatus,
                    cancellationToken);

                return ResponseViewModel<GRNResponseDto>.Success(
                    createdGrn,
                    "GRN created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating GRN for PO: {PurchaseOrderId}", request.Data.PurchaseOrderId);
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
            {
                throw new NotFoundException("GRN not found after creation", FinanceErrorCode.NotFound);
            }

            decimal totalRemaining = 0;
            var lineItemsResponse = new List<GRNLineItemResponseDto>();
            var inventoryImpact = new List<InventoryImpactDto>();

            foreach (var li in grn.LineItems)
            {
                decimal previousReceived = li.ReceivedQty - li.CurrentReceived;
                decimal remaining = li.OrderedQty - li.ReceivedQty;
                totalRemaining += remaining;

                // Get display name (Product > Service > Description)
                string displayName = (li.ProductName ?? li.ServiceName ?? li.Description ?? "Unknown Item")?.Trim();
                string displaySKU = (li.ProductSKU ?? "").Trim();

                if (string.IsNullOrWhiteSpace(displayName))
                    displayName = "Unknown Item";


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
                    UnitOfMeasure = null, // TODO: Add UnitOfMeasure field to Product/Service or POLineItem
                    Notes = li.Notes
                });

                // Only add inventory impact for Products (not Services)
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

            // Add payment warning if not fully paid
            if (paymentStatus != "Paid in Full" && paymentStatus != "Refunded")
            {
                warnings.Add($"Payment status is '{paymentStatus}'. Full payment required before closing PO.");
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
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"GRN-{year}-{nextNumber:D5}";
        }
    }
}
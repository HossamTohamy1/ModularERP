using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Shared.Interfaces;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Enum.Purchases_Enum;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_RefundInvoce
{
    public class CreateRefundFromPOHandler : IRequestHandler<CreateRefundFromPOCommand, ResponseViewModel<RefundDto>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IGeneralRepository<PurchaseOrder> _poRepo;
        private readonly IGeneralRepository<DebitNote> _debitNoteRepo;
        private readonly IGeneralRepository<GRNLineItem> _grnLineRepo;
        private readonly IGeneralRepository<POLineItem> _poLineRepo;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepo;
        private readonly IGeneralRepository<StockTransaction> _stockTxnRepo;
        private readonly IGeneralRepository<Supplier> _supplierRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateRefundFromPOHandler> _logger;

        public CreateRefundFromPOHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IGeneralRepository<PurchaseOrder> poRepo,
            IGeneralRepository<DebitNote> debitNoteRepo,
            IGeneralRepository<GRNLineItem> grnLineRepo,
            IGeneralRepository<POLineItem> poLineRepo,
            IGeneralRepository<WarehouseStock> warehouseStockRepo,
            IGeneralRepository<StockTransaction> stockTxnRepo,
            IGeneralRepository<Supplier> supplierRepo,
            IMapper mapper,
            ILogger<CreateRefundFromPOHandler> logger)
        {
            _refundRepo = refundRepo;
            _poRepo = poRepo;
            _debitNoteRepo = debitNoteRepo;
            _grnLineRepo = grnLineRepo;
            _poLineRepo = poLineRepo;
            _warehouseStockRepo = warehouseStockRepo;
            _stockTxnRepo = stockTxnRepo;
            _supplierRepo = supplierRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefundDto>> Handle(CreateRefundFromPOCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating refund from Purchase Order: {PurchaseOrderId}", request.PurchaseOrderId);

                // ============================================
                // STEP 1: VALIDATE PURCHASE ORDER
                // ============================================
                var purchaseOrder = await _poRepo.GetAll()
                    .Include(po => po.LineItems)
                    .Include(po => po.Supplier)
                    .Include(po => po.GoodsReceipts)
                        .ThenInclude(grn => grn.LineItems)
                    .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId, cancellationToken);

                if (purchaseOrder == null)
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Check if PO is cancelled
                if (purchaseOrder.DocumentStatus == DocumentStatus.Cancelled)
                {
                    throw new ValidationException(
                        "Cannot create refund for cancelled Purchase Order",
                        new Dictionary<string, string[]> { { "PurchaseOrderId", new[] { "PO is cancelled" } } },
                        "Purchases");
                }

                // ============================================
                // STEP 2: VALIDATE LINE ITEMS
                // ============================================
                if (!request.LineItems.Any())
                {
                    throw new ValidationException(
                        "Refund must contain at least one line item",
                        new Dictionary<string, string[]> { { "LineItems", new[] { "At least one line item is required" } } },
                        "Purchases");
                }

                var validationErrors = new Dictionary<string, string[]>();
                var lineItemsData = new List<(GRNLineItem grnLine, POLineItem poLine, Guid warehouseId, decimal alreadyReturned)>();

                foreach (var item in request.LineItems)
                {
                    // Get GRN Line Item with all navigation
                    var grnLine = await _grnLineRepo.GetAll()
                        .Include(g => g.GRN)
                            .ThenInclude(grn => grn.Warehouse)
                        .Include(g => g.POLineItem)
                            .ThenInclude(pol => pol.Product)
                        .FirstOrDefaultAsync(g => g.Id == item.GRNLineItemId, cancellationToken);

                    if (grnLine == null)
                    {
                        validationErrors.Add($"LineItem_{item.GRNLineItemId}",
                            new[] { "GRN line item not found" });
                        continue;
                    }

                    // Verify GRN is linked to this PO
                    if (grnLine.GRN.PurchaseOrderId != request.PurchaseOrderId)
                    {
                        validationErrors.Add($"LineItem_{item.GRNLineItemId}",
                            new[] { "GRN line item is not linked to this Purchase Order" });
                        continue;
                    }

                    // Get POLineItem
                    var poLine = grnLine.POLineItem;

                    // Calculate already returned quantity
                    var alreadyReturned = await _refundRepo.GetAll()
                        .Where(r => r.PurchaseOrderId == request.PurchaseOrderId)
                        .SelectMany(r => r.LineItems)
                        .Where(rl => rl.GRNLineItemId == item.GRNLineItemId)
                        .SumAsync(rl => rl.ReturnQuantity, cancellationToken);

                    var availableToReturn = grnLine.ReceivedQuantity - alreadyReturned;

                    // Validate return quantity
                    if (item.ReturnQuantity <= 0)
                    {
                        validationErrors.Add($"LineItem_{item.GRNLineItemId}",
                            new[] { "Return quantity must be greater than zero" });
                        continue;
                    }

                    if (item.ReturnQuantity > availableToReturn)
                    {
                        validationErrors.Add($"LineItem_{item.GRNLineItemId}",
                            new[] { $"Cannot return {item.ReturnQuantity} units. Only {availableToReturn} units available (Received: {grnLine.ReceivedQuantity}, Already Returned: {alreadyReturned})" });
                        continue;
                    }

                    // Validate unit price
                    if (item.UnitPrice != poLine.UnitPrice)
                    {
                        validationErrors.Add($"LineItem_{item.GRNLineItemId}",
                            new[] { $"Unit price mismatch. Expected: {poLine.UnitPrice}, Provided: {item.UnitPrice}" });
                        continue;
                    }

                    // Store for later use
                    lineItemsData.Add((grnLine, poLine, grnLine.GRN.WarehouseId, alreadyReturned));
                }

                if (validationErrors.Any())
                {
                    throw new ValidationException(
                        "Validation failed for refund line items",
                        validationErrors,
                        "Purchases");
                }

                // ============================================
                // STEP 3: CREATE REFUND
                // ============================================
                var refundCount = await _refundRepo.GetAll().CountAsync(cancellationToken);
                var refundNumber = $"REF-{DateTime.UtcNow:yyyyMMdd}-{refundCount + 1:D5}";
                var totalAmount = request.LineItems.Sum(item => item.ReturnQuantity * item.UnitPrice);

                var refund = new PurchaseRefund
                {
                    Id = Guid.NewGuid(),
                    RefundNumber = refundNumber,
                    PurchaseOrderId = request.PurchaseOrderId,
                    SupplierId = purchaseOrder.SupplierId,
                    RefundDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Reason = request.Reason,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                // Create Line Items
                for (int i = 0; i < request.LineItems.Count; i++)
                {
                    var lineItem = request.LineItems[i];
                    var (grnLine, poLine, warehouseId, _) = lineItemsData[i];

                    var refundLine = new RefundLineItem
                    {
                        Id = Guid.NewGuid(),
                        RefundId = refund.Id,
                        GRNLineItemId = lineItem.GRNLineItemId,
                        ReturnQuantity = lineItem.ReturnQuantity,
                        UnitPrice = lineItem.UnitPrice,
                        LineTotal = lineItem.ReturnQuantity * lineItem.UnitPrice,
                        CreatedAt = DateTime.UtcNow
                    };
                    refund.LineItems.Add(refundLine);
                }

                await _refundRepo.AddAsync(refund);

                // ============================================
                // STEP 4: UPDATE INVENTORY (WarehouseStock)
                // ============================================
                var inventoryImpacts = new List<InventoryImpactDto>();

                for (int i = 0; i < request.LineItems.Count; i++)
                {
                    var lineItem = request.LineItems[i];
                    var (grnLine, poLine, warehouseId, _) = lineItemsData[i];
                    var product = poLine.Product;

                    // Find or Create WarehouseStock record
                    var warehouseStock = await _warehouseStockRepo.GetAll()
                        .FirstOrDefaultAsync(ws => ws.WarehouseId == warehouseId
                                                && ws.ProductId == product.Id,
                                           cancellationToken);

                    if (warehouseStock == null)
                    {
                        _logger.LogWarning("WarehouseStock not found for Product {ProductId} in Warehouse {WarehouseId}. Creating new record.",
                            product.Id, warehouseId);

                        warehouseStock = new WarehouseStock
                        {
                            Id = Guid.NewGuid(),
                            WarehouseId = warehouseId,
                            ProductId = product.Id,
                            Quantity = 0,
                            AvailableQuantity = 0,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _warehouseStockRepo.AddAsync(warehouseStock);
                    }

                    // Decrease inventory
                    var oldQuantity = warehouseStock.Quantity;
                    warehouseStock.Quantity -= lineItem.ReturnQuantity;
                    warehouseStock.AvailableQuantity = warehouseStock.Quantity - (warehouseStock.ReservedQuantity ?? 0);
                    warehouseStock.LastStockOutDate = DateTime.UtcNow;
                    warehouseStock.UpdatedAt = DateTime.UtcNow;

                    await _warehouseStockRepo.Update(warehouseStock);

                    _logger.LogInformation("Warehouse Stock updated: Product {ProductId}, Warehouse {WarehouseId}, Old Qty: {OldQty}, Returned: {ReturnQty}, New Qty: {NewQty}",
                        product.Id, warehouseId, oldQuantity, lineItem.ReturnQuantity, warehouseStock.Quantity);

                    // Create Stock Transaction
                    var stockTransaction = new StockTransaction
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = purchaseOrder.CompanyId,
                        ProductId = product.Id,
                        WarehouseId = warehouseId,
                        TransactionType = StockTransactionType.Return,
                        Quantity = -lineItem.ReturnQuantity, // Negative for return
                        UnitCost = lineItem.UnitPrice,
                        StockLevelAfter = warehouseStock.Quantity,
                        ReferenceType = "REFUND",
                        ReferenceId = refund.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _stockTxnRepo.AddAsync(stockTransaction);

                    inventoryImpacts.Add(new InventoryImpactDto
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ProductSKU = product.SKU ?? "N/A",
                        WarehouseId = warehouseId,
                        QuantityReturned = lineItem.ReturnQuantity,
                        NewStockLevel = warehouseStock.Quantity
                    });
                }
                await _refundRepo.AddAsync(refund);
                await _refundRepo.SaveChanges(); 
                                // ============================================
                                // STEP 5: CREATE DEBIT NOTE & UPDATE SUPPLIER
                                // ============================================
                                DebitNote ? debitNote = null;

                if (request.CreateDebitNote)
                {
                    var debitNoteCount = await _debitNoteRepo.GetAll().CountAsync(cancellationToken);
                    var debitNoteNumber = $"DN-{DateTime.UtcNow:yyyyMMdd}-{debitNoteCount + 1:D5}";

                    debitNote = new DebitNote
                    {
                        Id = Guid.NewGuid(),
                        DebitNoteNumber = debitNoteNumber,
                        RefundId = refund.Id,
                        SupplierId = purchaseOrder.SupplierId,
                        NoteDate = DateTime.UtcNow,
                        Amount = totalAmount,
                        Notes = $"Auto-generated for Refund: {refundNumber}",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _debitNoteRepo.AddAsync(debitNote);
                    await _debitNoteRepo.SaveChanges(); // مهم


                    // Update Supplier Balance (if exists)
                    var supplier = await _supplierRepo.GetByID(purchaseOrder.SupplierId);
                    var balanceProperty = supplier.GetType().GetProperty("Balance");

                    if (balanceProperty != null)
                    {
                        var oldBalance = (decimal)(balanceProperty.GetValue(supplier) ?? 0m);
                        var newBalance = oldBalance - totalAmount;
                        balanceProperty.SetValue(supplier, newBalance);

                        var lastTxnProperty = supplier.GetType().GetProperty("LastTransactionDate");
                        lastTxnProperty?.SetValue(supplier, DateTime.UtcNow);

                        await _supplierRepo.Update(supplier);

                        _logger.LogInformation("Supplier balance updated: {SupplierId}, Old: {Old}, Adjusted: -{Amount}, New: {New}",
                            supplier.Id, oldBalance, totalAmount, newBalance);
                    }
                    else
                    {
                        _logger.LogWarning("Supplier model does not have Balance property. Skipping balance update.");
                    }

                    _logger.LogInformation("Created Debit Note: {DebitNoteNumber} for Refund: {RefundNumber}",
                        debitNoteNumber, refundNumber);
                }

                // ============================================
                // ⭐ STEP 6: SAVE CHANGES (BEFORE Status Update)
                // ============================================
                await _refundRepo.SaveChanges();

                _logger.LogInformation("Successfully created refund: {RefundNumber} from PO: {PONumber}",
                    refundNumber, purchaseOrder.PONumber);

                // ============================================
                // STEP 7: UPDATE PO STATUSES (AFTER Save)
                // ============================================
                var statusUpdates = await UpdatePurchaseOrderStatuses(purchaseOrder, cancellationToken);

                // ============================================
                // STEP 8: BUILD ENHANCED RESPONSE
                // ============================================
                var refundDto = await _refundRepo.GetAll()
                    .Include(r => r.PurchaseOrder)
                    .Include(r => r.Supplier)
                    .Include(r => r.DebitNote)
                        .ThenInclude(dn => dn.Supplier)
                    .Include(r => r.LineItems)
                        .ThenInclude(rl => rl.GRNLineItem)
                            .ThenInclude(grn => grn.POLineItem)
                                .ThenInclude(pol => pol.Product)
                    .Where(r => r.Id == refund.Id)
                    .Select(r => new RefundDto
                    {
                        Id = r.Id,
                        RefundNumber = r.RefundNumber,
                        PurchaseOrderId = r.PurchaseOrderId,
                        PurchaseOrderNumber = r.PurchaseOrder.PONumber,
                        SupplierId = r.SupplierId,
                        SupplierName = r.Supplier.Name,
                        RefundDate = r.RefundDate,
                        TotalAmount = r.TotalAmount,
                        Reason = r.Reason,
                        Notes = r.Notes,
                        HasDebitNote = r.DebitNote != null,

                        DebitNote = r.DebitNote != null ? new DebitNoteDto
                        {
                            Id = r.DebitNote.Id,
                            DebitNoteNumber = r.DebitNote.DebitNoteNumber,
                            RefundId = r.DebitNote.RefundId,
                            SupplierId = r.DebitNote.SupplierId,
                            SupplierName = r.DebitNote.Supplier.Name,
                            Amount = r.DebitNote.Amount,
                            NoteDate = r.DebitNote.NoteDate,
                            Notes = r.DebitNote.Notes
                        } : null,

                        LineItems = r.LineItems.Select(rl => new RefundLineItemDto
                        {
                            Id = rl.Id,
                            RefundId = rl.RefundId,
                            GRNLineItemId = rl.GRNLineItemId,
                            ProductId = (Guid)rl.GRNLineItem.POLineItem.ProductId,
                            ProductName = rl.GRNLineItem.POLineItem.Product.Name,
                            ProductSKU = rl.GRNLineItem.POLineItem.Product.SKU ?? "N/A",
                            ReturnQuantity = rl.ReturnQuantity,
                            UnitPrice = rl.UnitPrice,
                            LineTotal = rl.LineTotal
                        }).ToList(),

                        StatusUpdates = statusUpdates,
                        InventoryImpact = inventoryImpacts,

                        CreatedAt = r.CreatedAt
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (refundDto == null)
                {
                    throw new NotFoundException(
                        $"Refund with ID {refund.Id} not found after creation",
                        FinanceErrorCode.NotFound);
                }

                return ResponseViewModel<RefundDto>.Success(
                    refundDto,
                    "Refund created successfully and inventory updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund from PO: {PurchaseOrderId}", request.PurchaseOrderId);
                throw;
            }
        }

        /// <summary>
        /// Update PO Reception and Payment Statuses based on current state
        /// </summary>
        private async Task<StatusUpdatesDto> UpdatePurchaseOrderStatuses(
            PurchaseOrder po,
            CancellationToken cancellationToken)
        {
            // ⭐ Reload PO to get fresh data after SaveChanges
            var purchaseOrder = await _poRepo.GetAll()
                .Include(p => p.LineItems)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == po.Id, cancellationToken);

            if (purchaseOrder == null)
            {
                throw new NotFoundException(
                    $"Purchase Order {po.Id} not found",
                    FinanceErrorCode.NotFound);
            }

            // ============================================
            // CALCULATE RECEPTION STATUS
            // ============================================
            var totalOrdered = purchaseOrder.LineItems.Sum(li => li.Quantity);

            var totalReceived = await _grnLineRepo.GetAll()
                .Include(g => g.GRN)
                .Where(g => g.GRN.PurchaseOrderId == po.Id)
                .SumAsync(g => g.ReceivedQuantity, cancellationToken);

            // ⭐ Now includes the newly created refund
            var totalReturned = await _refundRepo.GetAll()
                .Where(r => r.PurchaseOrderId == po.Id)
                .SelectMany(r => r.LineItems)
                .SumAsync(rl => rl.ReturnQuantity, cancellationToken);

            var netReceived = totalReceived - totalReturned;

            string receptionStatus;
            if (totalReturned > 0 && netReceived == 0)
            {
                receptionStatus = "Returned";
            }
            else if (netReceived == 0)
            {
                receptionStatus = "NotReceived";
            }
            else if (netReceived < totalOrdered)
            {
                receptionStatus = "PartiallyReceived";
            }
            else
            {
                receptionStatus = "FullyReceived";
            }

            // ============================================
            // CALCULATE PAYMENT STATUS
            // ============================================
            var totalPayable = purchaseOrder.TotalAmount;

            // ⭐ Now includes the new refund
            var totalRefunded = await _refundRepo.GetAll()
                .Where(r => r.PurchaseOrderId == po.Id)
                .SumAsync(r => r.TotalAmount, cancellationToken);

            decimal totalPaid = purchaseOrder.DepositAmount;
            // TODO: Add actual payments from invoices

            var netPayable = totalPayable - totalRefunded;
            var netPaid = totalPaid;

            string paymentStatus;
            if (totalRefunded > 0 && totalRefunded >= totalPaid && totalPaid > 0)
            {
                paymentStatus = "Refunded";
            }
            else if (netPaid == 0)
            {
                paymentStatus = "Unpaid";
            }
            else if (netPaid < netPayable)
            {
                paymentStatus = "PartiallyPaid";
            }
            else
            {
                paymentStatus = "PaidInFull";
            }

            // ============================================
            // UPDATE PO
            // ============================================
            purchaseOrder.ReceptionStatus = Enum.Parse<ReceptionStatus>(receptionStatus);
            purchaseOrder.PaymentStatus = Enum.Parse<PaymentStatus>(paymentStatus);
            purchaseOrder.UpdatedAt = DateTime.UtcNow;
            await _poRepo.Update(purchaseOrder);
            await _poRepo.SaveChanges(); // ⭐ Save PO updates

            _logger.LogInformation("PO Status Updated: {PONumber} - Reception: {Reception}, Payment: {Payment}",
                purchaseOrder.PONumber, receptionStatus, paymentStatus);

            // Get supplier balance
            var supplier = await _supplierRepo.GetByID(po.SupplierId);
            var balanceProperty = supplier?.GetType().GetProperty("Balance");
            var supplierBalance = balanceProperty != null
                ? (decimal)(balanceProperty.GetValue(supplier) ?? 0m)
                : 0m;

            return new StatusUpdatesDto
            {
                PurchaseOrder = new POStatusDto
                {
                    ReceptionStatus = receptionStatus,
                    PaymentStatus = paymentStatus,
                    DocumentStatus = purchaseOrder.DocumentStatus.ToString(),
                    TotalOrdered = totalOrdered,
                    TotalReceived = totalReceived,
                    TotalReturned = totalReturned, // ⭐ Now correct
                    NetReceived = netReceived       // ⭐ Now correct
                },
                Supplier = new SupplierStatusDto
                {
                    SupplierId = po.SupplierId,
                    SupplierName = purchaseOrder.Supplier?.Name ?? "Unknown",
                    BalanceAdjusted = -totalRefunded, // ⭐ Now correct
                    NewBalance = supplierBalance
                }
            };
        }
    }
}
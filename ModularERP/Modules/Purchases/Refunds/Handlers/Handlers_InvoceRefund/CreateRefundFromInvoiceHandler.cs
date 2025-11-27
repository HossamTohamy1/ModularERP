using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Shared.Interfaces;
using Azure.Core;
using ModularERP.Common.Enum.Purchases_Enum;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_InvoceRefund
{
    public class CreateRefundFromInvoiceHandler : IRequestHandler<CreateRefundFromInvoiceCommand, ResponseViewModel<RefundDto>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepo;
        private readonly IGeneralRepository<PurchaseOrder> _poRepo;
        private readonly IGeneralRepository<POLineItem> _poLineItemRepo;
        private readonly IGeneralRepository<DebitNote> _debitNoteRepo;
        private readonly IGeneralRepository<GRNLineItem> _grnLineRepo;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepo;
        private readonly IGeneralRepository<StockTransaction> _stockTransactionRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateRefundFromInvoiceHandler> _logger;

        public CreateRefundFromInvoiceHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IGeneralRepository<PurchaseInvoice> invoiceRepo,
            IGeneralRepository<PurchaseOrder> poRepo,
            IGeneralRepository<POLineItem> poLineItemRepo,
            IGeneralRepository<DebitNote> debitNoteRepo,
            IGeneralRepository<GRNLineItem> grnLineRepo,
            IGeneralRepository<WarehouseStock> warehouseStockRepo,
            IGeneralRepository<StockTransaction> stockTransactionRepo,
            IMapper mapper,
            ILogger<CreateRefundFromInvoiceHandler> logger)
        {
            _refundRepo = refundRepo;
            _invoiceRepo = invoiceRepo;
            _poRepo = poRepo;
            _poLineItemRepo = poLineItemRepo;
            _debitNoteRepo = debitNoteRepo;
            _grnLineRepo = grnLineRepo;
            _warehouseStockRepo = warehouseStockRepo;
            _stockTransactionRepo = stockTransactionRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefundDto>> Handle(CreateRefundFromInvoiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating refund from Purchase Invoice: {InvoiceId}", request.InvoiceId);

                // ✅ 1. Validate Invoice exists and load all relations
                var invoice = await _invoiceRepo.GetAll()
                    .Include(i => i.PurchaseOrder)
                        .ThenInclude(po => po.LineItems)
                    .Include(i => i.Supplier)
                    .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

                if (invoice == null)
                {
                    throw new NotFoundException(
                        $"Purchase Invoice with ID {request.InvoiceId} not found",
                        FinanceErrorCode.NotFound);
                }

                // ✅ 2. Validate PO Status
                if (invoice.PurchaseOrder.DocumentStatus == DocumentStatus.Cancelled)
                {
                    throw new BusinessLogicException(
                        "Cannot create refund for a cancelled Purchase Order",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                if (invoice.PurchaseOrder.DocumentStatus == DocumentStatus.Closed)
                {
                    throw new BusinessLogicException(
                        "Cannot create refund for a closed Purchase Order. Please reopen it first.",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // ✅ 3. Validate Line Items
                if (!request.LineItems.Any())
                {
                    throw new ValidationException(
                        "Refund must contain at least one line item",
                        new Dictionary<string, string[]> { { "LineItems", new[] { "At least one line item is required" } } },
                        "Purchases");
                }

                // ✅ 4. Validate Return Quantities & Collect GRN data
                var grnLineItems = new List<GRNLineItem>();
                foreach (var lineItem in request.LineItems)
                {
                    var grnLine = await _grnLineRepo.GetAll()
                        .Include(g => g.GRN)
                            .ThenInclude(grn => grn.Warehouse)
                        .Include(g => g.POLineItem)
                            .ThenInclude(pol => pol.Product)
                        .Include(g => g.POLineItem)
                            .ThenInclude(pol => pol.Service)
                        .FirstOrDefaultAsync(g => g.Id == lineItem.GRNLineItemId, cancellationToken);

                    if (grnLine == null)
                    {
                        throw new NotFoundException(
                            $"GRN Line Item with ID {lineItem.GRNLineItemId} not found",
                            FinanceErrorCode.NotFound);
                    }

                    // ✅ CRITICAL VALIDATION: Verify GRN is linked to THIS PO
                    if (grnLine.GRN.PurchaseOrderId != invoice.PurchaseOrderId)
                    {
                        throw new BusinessLogicException(
                            $"GRN Line Item {lineItem.GRNLineItemId} belongs to PO {grnLine.GRN.PurchaseOrderId}, but invoice is for PO {invoice.PurchaseOrderId}. Cannot create refund across different Purchase Orders.",
                            "Purchases",
                            FinanceErrorCode.BusinessLogicError);
                    }

                    _logger.LogInformation(
                        "Validated GRNLineItem {GRNLineId}: GRN.POId={GRNPOId}, Invoice.POId={InvoicePOId}, Match={Match}",
                        grnLine.Id,
                        grnLine.GRN.PurchaseOrderId,
                        invoice.PurchaseOrderId,
                        grnLine.GRN.PurchaseOrderId == invoice.PurchaseOrderId);

                    // Calculate already returned quantity
                    var alreadyReturned = await _refundRepo.GetAll()
                        .Where(r => r.PurchaseOrderId == invoice.PurchaseOrderId)
                        .SelectMany(r => r.LineItems)
                        .Where(rl => rl.GRNLineItemId == lineItem.GRNLineItemId)
                        .SumAsync(rl => rl.ReturnQuantity, cancellationToken);

                    var availableToReturn = grnLine.ReceivedQuantity - alreadyReturned;

                    if (lineItem.ReturnQuantity > availableToReturn)
                    {
                        throw new BusinessLogicException(
                            $"Return quantity ({lineItem.ReturnQuantity}) exceeds available quantity ({availableToReturn}) for product {grnLine.POLineItem.Product?.Name ?? grnLine.POLineItem.Service?.Name ?? "Unknown"}",
                            "Purchases",
                            FinanceErrorCode.BusinessLogicError);
                    }

                    if (lineItem.ReturnQuantity <= 0)
                    {
                        throw new ValidationException(
                            "Return quantity must be greater than zero",
                            new Dictionary<string, string[]> {
                                { $"LineItems[{lineItem.GRNLineItemId}].ReturnQuantity",
                                  new[] { "Quantity must be positive" } }
                            },
                            "Purchases");
                    }

                    grnLineItems.Add(grnLine);
                }

                // ✅ 5. Generate Refund Number
                var refundCount = await _refundRepo.GetAll().CountAsync(cancellationToken);
                var refundNumber = $"REF-{DateTime.UtcNow:yyyyMMdd}-{refundCount + 1:D5}";

                // ✅ 6. Calculate Total Amount
                var totalAmount = request.LineItems.Sum(item => item.ReturnQuantity * item.UnitPrice);

                // ✅ 7. Create Refund Entity
                var refund = new PurchaseRefund
                {
                    Id = Guid.NewGuid(),
                    RefundNumber = refundNumber,
                    PurchaseOrderId = invoice.PurchaseOrderId,
                    PurchaseInvoiceId = invoice.Id,
                    SupplierId = invoice.SupplierId,
                    RefundDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Reason = request.Reason,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                // ✅ 8. Create Refund Line Items & Update Inventory
                var inventoryImpacts = new List<InventoryImpactDto>();

                for (int i = 0; i < request.LineItems.Count; i++)
                {
                    var lineItem = request.LineItems[i];
                    var grnLine = grnLineItems[i];

                    // Create refund line
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

                    // ✅ Update POLineItem.ReturnedQuantity (for consistency, even though not used in calculations)
                    var poLineItem = await _poLineItemRepo.GetAll()
                        .FirstOrDefaultAsync(pl => pl.Id == grnLine.POLineItemId, cancellationToken);

                    if (poLineItem != null)
                    {
                        poLineItem.ReturnedQuantity += lineItem.ReturnQuantity;
                    }

                    // Update Warehouse Stock (decrease quantity)
                    if (grnLine.POLineItem.ProductId.HasValue)
                    {
                        var warehouseStock = await _warehouseStockRepo.GetAll()
                            .FirstOrDefaultAsync(ws =>
                                ws.WarehouseId == grnLine.GRN.WarehouseId &&
                                ws.ProductId == grnLine.POLineItem.ProductId.Value,
                                cancellationToken);

                        if (warehouseStock != null)
                        {
                            // Decrease stock
                            warehouseStock.Quantity -= lineItem.ReturnQuantity;
                            warehouseStock.AvailableQuantity = warehouseStock.Quantity - (warehouseStock.ReservedQuantity ?? 0);
                            warehouseStock.TotalValue = warehouseStock.Quantity * (warehouseStock.AverageUnitCost ?? 0);
                            warehouseStock.LastStockOutDate = DateTime.UtcNow;
                            warehouseStock.UpdatedAt = DateTime.UtcNow;

                            // Create stock transaction (Outbound)
                            var stockTransaction = new StockTransaction
                            {
                                Id = Guid.NewGuid(),
                                CompanyId = invoice.CompanyId,
                                ProductId = grnLine.POLineItem.ProductId.Value,
                                WarehouseId = grnLine.GRN.WarehouseId,
                                TransactionType = StockTransactionType.Return,
                                Quantity = -lineItem.ReturnQuantity,
                                UnitCost = lineItem.UnitPrice,
                                StockLevelAfter = warehouseStock.Quantity,
                                ReferenceType = "PurchaseRefund",
                                ReferenceId = refund.Id,
                                CreatedAt = DateTime.UtcNow
                            };

                            await _stockTransactionRepo.AddAsync(stockTransaction);

                            // Add to inventory impact response
                            inventoryImpacts.Add(new InventoryImpactDto
                            {
                                ProductId = grnLine.POLineItem.ProductId.Value,
                                ProductName = grnLine.POLineItem.Product?.Name ?? "Unknown",
                                ProductSKU = grnLine.POLineItem.Product?.SKU ?? "N/A",
                                WarehouseId = grnLine.GRN.WarehouseId,
                                QuantityReturned = lineItem.ReturnQuantity,
                                NewStockLevel = warehouseStock.Quantity
                            });
                        }
                    }
                }

                // ✅ Save all changes BEFORE creating dependent entities
                await _poLineItemRepo.SaveChanges();
                await _warehouseStockRepo.SaveChanges();
                await _stockTransactionRepo.SaveChanges();

                // ✅ CRITICAL: Save Refund FIRST (so DebitNote can reference it)
                await _refundRepo.AddAsync(refund);
                await _refundRepo.SaveChanges();

                _logger.LogInformation("Refund saved to database: {RefundNumber}", refundNumber);

                // ✅ 9. Create Debit Note (AFTER Refund is saved)
                DebitNote? debitNote = null;
                if (request.CreateDebitNote)
                {
                    var debitNoteCount = await _debitNoteRepo.GetAll().CountAsync(cancellationToken);
                    var debitNoteNumber = $"DN-{DateTime.UtcNow:yyyyMMdd}-{debitNoteCount + 1:D5}";

                    debitNote = new DebitNote
                    {
                        Id = Guid.NewGuid(),
                        DebitNoteNumber = debitNoteNumber,
                        RefundId = refund.Id,  // ← Now safe, Refund exists in DB
                        SupplierId = invoice.SupplierId,
                        NoteDate = DateTime.UtcNow,
                        Amount = totalAmount,
                        Notes = $"Auto-generated for Refund: {refundNumber} | Invoice: {invoice.InvoiceNumber} | PO: {invoice.PurchaseOrder.PONumber}",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _debitNoteRepo.AddAsync(debitNote);
                    await _debitNoteRepo.SaveChanges();

                    _logger.LogInformation("Created Debit Note: {DebitNoteNumber} for Refund: {RefundNumber}",
                        debitNoteNumber, refundNumber);
                }

                // ✅ 10. Update PO Statuses (BRSD State Machine)
                // Now calculations will include the newly saved refund
                var statusUpdates = await UpdatePurchaseOrderStatuses(invoice.PurchaseOrder.Id, request, cancellationToken);

                // ✅ 11. Update Invoice Payment Status if needed
                if (invoice.PaymentStatus == PaymentStatus.PaidInFull && totalAmount > 0)
                {
                    invoice.AmountDue += totalAmount;
                    invoice.PaymentStatus = PaymentStatus.PartiallyPaid;
                    await _invoiceRepo.SaveChanges();
                }

                _logger.LogInformation("Successfully created refund: {RefundNumber} from Invoice: {InvoiceId}",
                    refundNumber, request.InvoiceId);

                // ✅ 12. Build Complete Response DTO
                var refundDto = new RefundDto
                {
                    Id = refund.Id,
                    RefundNumber = refund.RefundNumber,
                    PurchaseOrderId = refund.PurchaseOrderId,
                    PurchaseOrderNumber = invoice.PurchaseOrder.PONumber,
                    SupplierId = refund.SupplierId,
                    SupplierName = invoice.Supplier.Name,
                    RefundDate = refund.RefundDate,
                    TotalAmount = refund.TotalAmount,
                    Reason = refund.Reason,
                    Notes = refund.Notes,
                    HasDebitNote = debitNote != null,
                    DebitNote = debitNote != null ? new DebitNoteDto
                    {
                        Id = debitNote.Id,
                        DebitNoteNumber = debitNote.DebitNoteNumber,
                        RefundId = debitNote.RefundId,
                        SupplierId = debitNote.SupplierId,
                        SupplierName = invoice.Supplier.Name,
                        NoteDate = debitNote.NoteDate,
                        Amount = debitNote.Amount,
                        Notes = debitNote.Notes
                    } : null,
                    LineItems = refund.LineItems.Select((rl, idx) =>
                    {
                        var grnLine = grnLineItems[idx];
                        return new RefundLineItemDto
                        {
                            Id = rl.Id,
                            RefundId = rl.RefundId,
                            GRNLineItemId = rl.GRNLineItemId,
                            ProductId = grnLine.POLineItem.ProductId ?? Guid.Empty,
                            ProductName = grnLine.POLineItem.Product?.Name ??
                                         grnLine.POLineItem.Service?.Name ??
                                         "Unknown Item",
                            ProductSKU = grnLine.POLineItem.Product?.SKU ?? "N/A",
                            ReturnQuantity = rl.ReturnQuantity,
                            UnitPrice = rl.UnitPrice,
                            LineTotal = rl.LineTotal
                        };
                    }).ToList(),
                    StatusUpdates = new StatusUpdatesDto
                    {
                        PurchaseOrder = statusUpdates,
                        Supplier = new SupplierStatusDto
                        {
                            SupplierId = invoice.SupplierId,
                            SupplierName = invoice.Supplier.Name,
                            BalanceAdjusted = -totalAmount,
                            NewBalance = 0
                        }
                    },
                    InventoryImpact = inventoryImpacts,
                    CreatedAt = refund.CreatedAt
                };

                return ResponseViewModel<RefundDto>.Success(
                    refundDto,
                    "Refund created successfully from Purchase Invoice");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund from Invoice: {InvoiceId}", request.InvoiceId);
                throw;
            }
        }

        /// <summary>
        /// Updates PO statuses based on BRSD business rules
        /// Uses same approach as CreateRefundFromPOHandler - calculates from Refund table directly
        /// </summary>
        private async Task<POStatusDto> UpdatePurchaseOrderStatuses(
            Guid purchaseOrderId,
            CreateRefundFromInvoiceCommand request,  
            CancellationToken cancellationToken)

        {
            var purchaseOrder = await _poRepo.GetAll()
                .Include(p => p.LineItems)
                .FirstOrDefaultAsync(p => p.Id == purchaseOrderId, cancellationToken);

            if (purchaseOrder == null) return null!;

            // ============================================
            // ✅ CALCULATE RECEPTION STATUS
            // Use GRN Line Items from THIS refund's validated GRNs only
            // ============================================
            var totalOrdered = purchaseOrder.LineItems.Sum(li => li.Quantity);

            // ✅ Calculate received ONLY from GRNs linked to validated refund line items
            // This ensures we only count GRNs that were actually used in refunds
            var refundGrnLineIds = request.LineItems.Select(li => li.GRNLineItemId).ToList();

            var totalReceived = await _grnLineRepo.GetAll()
                .Include(g => g.GRN)
                .Where(g => g.GRN != null &&
                           g.GRN.PurchaseOrderId == purchaseOrderId)
                .SumAsync(g => g.ReceivedQuantity, cancellationToken);

            // ✅ Get all GRN line IDs for this PO (with proper null checks)
            var poGrnLineIds = await _grnLineRepo.GetAll()
                .Include(g => g.GRN)
                .Where(g => g.GRN != null && g.GRN.PurchaseOrderId == purchaseOrderId)
                .Select(g => g.Id)
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "PO {PONumber}: Found {Count} GRN line items linked to this PO",
                purchaseOrder.PONumber, poGrnLineIds.Count);

            // ✅ Calculate total returned ONLY for this PO's GRN lines
            var totalReturned = await _refundRepo.GetAll()
                .Where(r => r.PurchaseOrderId == purchaseOrderId)
                .SelectMany(r => r.LineItems)
                .Where(rl => poGrnLineIds.Contains(rl.GRNLineItemId))
                .SumAsync(rl => rl.ReturnQuantity, cancellationToken);

            var netReceived = totalReceived - totalReturned;

            _logger.LogInformation(
                "PO {PONumber} Calculations: Ordered={Ordered}, Received={Received}, Returned={Returned}, Net={Net}, GRNLineCount={GRNCount}",
                purchaseOrder.PONumber, totalOrdered, totalReceived, totalReturned, netReceived, poGrnLineIds.Count);

            // ✅ VALIDATION: Detect data anomalies
            if (totalReturned > totalReceived)
            {
                _logger.LogError(
                    "DATA CORRUPTION for PO {PONumber}: totalReturned ({Returned}) > totalReceived ({Received}). Some refunds reference GRNs not linked to this PO!",
                    purchaseOrder.PONumber, totalReturned, totalReceived);
            }

            if (poGrnLineIds.Count == 0 && totalReturned > 0)
            {
                _logger.LogError(
                    "DATA CORRUPTION for PO {PONumber}: No GRN lines found, but {Returned} items marked as returned!",
                    purchaseOrder.PONumber, totalReturned);
            }

            // Update Reception Status (BRSD Rules)
            string previousReceptionStatus = purchaseOrder.ReceptionStatus.ToString();

            if (totalReturned > 0 && netReceived == 0)
            {
                purchaseOrder.ReceptionStatus = ReceptionStatus.Returned;
            }
            else if (netReceived == 0)
            {
                purchaseOrder.ReceptionStatus = ReceptionStatus.NotReceived;
            }
            else if (netReceived < totalOrdered)
            {
                purchaseOrder.ReceptionStatus = ReceptionStatus.PartiallyReceived;
            }
            else
            {
                purchaseOrder.ReceptionStatus = ReceptionStatus.FullyReceived;
            }

            // ============================================
            // ✅ CALCULATE PAYMENT STATUS
            // ============================================
            var totalPayable = purchaseOrder.TotalAmount;

            var totalRefunded = await _refundRepo.GetAll()
                .Where(r => r.PurchaseOrderId == purchaseOrderId)
                .SumAsync(r => r.TotalAmount, cancellationToken);

            decimal totalPaid = purchaseOrder.DepositAmount;
            // TODO: Add actual payments from invoices

            var netPayable = totalPayable - totalRefunded;
            var netPaid = totalPaid;

            if (totalRefunded > 0 && totalRefunded >= totalPaid && totalPaid > 0)
            {
                purchaseOrder.PaymentStatus = PaymentStatus.Refunded;
            }
            else if (netPaid == 0)
            {
                purchaseOrder.PaymentStatus = PaymentStatus.Unpaid;
            }
            else if (netPaid < netPayable)
            {
                purchaseOrder.PaymentStatus = PaymentStatus.PartiallyPaid;
            }
            else
            {
                purchaseOrder.PaymentStatus = PaymentStatus.PaidInFull;
            }

            purchaseOrder.UpdatedAt = DateTime.UtcNow;
            await _poRepo.SaveChanges();

            _logger.LogInformation(
                "Updated PO {PONumber} statuses: Reception {OldReception} → {NewReception}, Payment: {Payment}",
                purchaseOrder.PONumber,
                previousReceptionStatus,
                purchaseOrder.ReceptionStatus,
                purchaseOrder.PaymentStatus);

            return new POStatusDto
            {
                ReceptionStatus = purchaseOrder.ReceptionStatus.ToString(),
                PaymentStatus = purchaseOrder.PaymentStatus.ToString(),
                DocumentStatus = purchaseOrder.DocumentStatus.ToString(),
                TotalOrdered = totalOrdered,
                TotalReceived = totalReceived,
                TotalReturned = totalReturned,
                NetReceived = netReceived
            };
        }
    }
}
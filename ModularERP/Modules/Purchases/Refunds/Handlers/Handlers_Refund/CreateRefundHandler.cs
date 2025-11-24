using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_Refund
{
    public class CreateRefundHandler : IRequestHandler<CreateRefundCommand, ResponseViewModel<RefundDto>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IGeneralRepository<DebitNote> _debitNoteRepo;
        private readonly IGeneralRepository<PurchaseOrder> _poRepo;
        private readonly IGeneralRepository<GRNLineItem> _grnLineRepo;
        private readonly IGeneralRepository<POLineItem> _poLineRepo;
        private readonly IGeneralRepository<WarehouseStock> _stockRepo;
        private readonly IGeneralRepository<StockTransaction> _stockTransactionRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateRefundHandler> _logger;

        public CreateRefundHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IGeneralRepository<DebitNote> debitNoteRepo,
            IGeneralRepository<PurchaseOrder> poRepo,
            IGeneralRepository<GRNLineItem> grnLineRepo,
            IGeneralRepository<POLineItem> poLineRepo,
            IGeneralRepository<WarehouseStock> stockRepo,
            IGeneralRepository<StockTransaction> stockTransactionRepo,
            IMapper mapper,
            ILogger<CreateRefundHandler> logger)
        {
            _refundRepo = refundRepo;
            _debitNoteRepo = debitNoteRepo;
            _poRepo = poRepo;
            _grnLineRepo = grnLineRepo;
            _poLineRepo = poLineRepo;
            _stockRepo = stockRepo;
            _stockTransactionRepo = stockTransactionRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefundDto>> Handle(CreateRefundCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting refund creation for PO: {PurchaseOrderId}", request.PurchaseOrderId);

                // === 1. VALIDATION ===
                if (!request.LineItems.Any())
                {
                    throw new ValidationException(
                        "Refund must contain at least one line item",
                        new Dictionary<string, string[]> { { "LineItems", new[] { "At least one line item is required" } } },
                        "Purchases");
                }

                // Load PO with all needed data
                var purchaseOrder = await _poRepo.GetAll()
                    .Include(po => po.LineItems)
                    .Include(po => po.Supplier)
                    .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId, cancellationToken);

                if (purchaseOrder == null)
                {
                    throw new ValidationException(
                        "Purchase Order not found",
                        new Dictionary<string, string[]> { { "PurchaseOrderId", new[] { "Invalid Purchase Order" } } },
                        "Purchases");
                }

                // Validate each line item and check quantities
                var validationErrors = new Dictionary<string, string[]>();
                var grnLineItems = new List<GRNLineItem>();

                foreach (var lineItem in request.LineItems)
                {
                    var grnLine = await _grnLineRepo.GetAll()
                        .Include(g => g.POLineItem)
                        .ThenInclude(pol => pol.Product)
                        .Include(g => g.GRN)
                        .FirstOrDefaultAsync(g => g.Id == lineItem.GRNLineItemId, cancellationToken);

                    if (grnLine == null)
                    {
                        validationErrors.Add($"GRNLineItem_{lineItem.GRNLineItemId}",
                            new[] { "GRN Line Item not found" });
                        continue;
                    }

                    // Check if return quantity exceeds received quantity
                    var alreadyReturned = await _refundRepo.GetAll()
                        .Where(r => r.PurchaseOrderId == request.PurchaseOrderId)
                        .SelectMany(r => r.LineItems)
                        .Where(rl => rl.GRNLineItemId == lineItem.GRNLineItemId)
                        .SumAsync(rl => rl.ReturnQuantity, cancellationToken);

                    var availableToReturn = grnLine.ReceivedQuantity - alreadyReturned;

                    if (lineItem.ReturnQuantity > availableToReturn)
                    {
                        validationErrors.Add($"LineItem_{lineItem.GRNLineItemId}",
                            new[] { $"Return quantity ({lineItem.ReturnQuantity}) exceeds available quantity ({availableToReturn})" });
                    }

                    grnLineItems.Add(grnLine);
                }

                if (validationErrors.Any())
                {
                    throw new ValidationException(
                        "Validation failed for refund line items",
                        validationErrors,
                        "Purchases");
                }

                // === 2. GENERATE REFUND NUMBER ===
                var refundCount = await _refundRepo.GetAll().CountAsync(cancellationToken);
                var refundNumber = $"REF-{DateTime.UtcNow:yyyyMMdd}-{refundCount + 1:D5}";

                // === 3. CALCULATE TOTAL ===
                var totalAmount = request.LineItems.Sum(item => item.ReturnQuantity * item.UnitPrice);

                // === 4. CREATE REFUND ENTITY ===
                var refund = new PurchaseRefund
                {
                    Id = Guid.NewGuid(),
                    RefundNumber = refundNumber,
                    PurchaseOrderId = request.PurchaseOrderId,
                    SupplierId = request.SupplierId,
                    RefundDate = request.RefundDate,
                    TotalAmount = totalAmount,
                    Reason = request.Reason,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                // === 5. CREATE LINE ITEMS (without updating POLineItem yet) ===
                foreach (var lineItem in request.LineItems)
                {
                    var grnLine = grnLineItems.First(g => g.Id == lineItem.GRNLineItemId);

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

                // === 6. CREATE DEBIT NOTE (ALWAYS - as per BRSD) ===
                var debitNoteCount = await _debitNoteRepo.GetAll().CountAsync(cancellationToken);
                var debitNoteNumber = $"DN-{DateTime.UtcNow:yyyyMMdd}-{debitNoteCount + 1:D5}";

                var debitNote = new DebitNote
                {
                    Id = Guid.NewGuid(),
                    DebitNoteNumber = debitNoteNumber,
                    RefundId = refund.Id,
                    SupplierId = request.SupplierId,
                    NoteDate = DateTime.UtcNow,
                    Amount = totalAmount,
                    Notes = $"Auto-generated for Refund: {refundNumber}",
                    CreatedAt = DateTime.UtcNow
                };

                await _debitNoteRepo.AddAsync(debitNote);
                _logger.LogInformation("Created Debit Note: {DebitNoteNumber} for Refund: {RefundNumber}",
                    debitNoteNumber, refundNumber);

                // === 7. SAVE REFUND & DEBIT NOTE FIRST ===
                await _refundRepo.SaveChanges();

                // === 8. NOW UPDATE INVENTORY & PO LINE ITEMS ===
                var inventoryImpacts = new List<InventoryImpactDto>();

                foreach (var lineItem in request.LineItems)
                {
                    var grnLine = grnLineItems.First(g => g.Id == lineItem.GRNLineItemId);

                    // Update POLineItem returned quantity
                    // IMPORTANT: Recalculate from database AFTER saving the refund
                    var poLineItem = await _poLineRepo.GetAll()
                        .Include(pol => pol.RefundLineItems)
                        .ThenInclude(rl => rl.GRNLineItem)
                        .FirstOrDefaultAsync(pol => pol.Id == grnLine.POLineItemId, cancellationToken);

                    if (poLineItem != null)
                    {
                        // Calculate total returned from all refund lines linked to this POLineItem
                        var totalReturnedForThisLine = poLineItem.RefundLineItems
                            .Where(rl => rl.GRNLineItem.POLineItemId == poLineItem.Id)
                            .Sum(rl => rl.ReturnQuantity);

                        poLineItem.ReturnedQuantity = totalReturnedForThisLine;
                        poLineItem.RemainingQuantity = poLineItem.Quantity - poLineItem.ReceivedQuantity + poLineItem.ReturnedQuantity;

                        await _poLineRepo.Update(poLineItem);

                        _logger.LogInformation(
                            "Updated POLineItem {POLineItemId}: Ordered={Ordered}, Received={Received}, Returned={Returned}, Remaining={Remaining}",
                            poLineItem.Id,
                            poLineItem.Quantity,
                            poLineItem.ReceivedQuantity,
                            poLineItem.ReturnedQuantity,
                            poLineItem.RemainingQuantity);
                    }

                    // Update Inventory (decrease stock)
                    if (grnLine.POLineItem?.ProductId != null)
                    {
                        var warehouseId = grnLine.GRN.WarehouseId;
                        var productId = grnLine.POLineItem.ProductId.Value;

                        var warehouseStock = await _stockRepo.GetAll()
                            .FirstOrDefaultAsync(ws => ws.ProductId == productId && ws.WarehouseId == warehouseId, cancellationToken);

                        if (warehouseStock != null)
                        {
                            warehouseStock.Quantity -= lineItem.ReturnQuantity;
                            warehouseStock.AvailableQuantity = warehouseStock.Quantity - (warehouseStock.ReservedQuantity ?? 0);
                            warehouseStock.TotalValue = warehouseStock.Quantity * (warehouseStock.AverageUnitCost ?? 0);
                            warehouseStock.LastStockOutDate = DateTime.UtcNow;
                            await _stockRepo.Update(warehouseStock);

                            // Create Stock Transaction
                            var stockTransaction = new StockTransaction
                            {
                                Id = Guid.NewGuid(),
                                CompanyId = purchaseOrder.CompanyId,
                                ProductId = productId,
                                WarehouseId = warehouseId,
                                TransactionType = StockTransactionType.Return,
                                Quantity = -lineItem.ReturnQuantity,
                                UnitCost = lineItem.UnitPrice,
                                StockLevelAfter = warehouseStock.Quantity,
                                ReferenceType = "PurchaseRefund",
                                ReferenceId = refund.Id,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _stockTransactionRepo.AddAsync(stockTransaction);

                            inventoryImpacts.Add(new InventoryImpactDto
                            {
                                ProductId = productId,
                                ProductName = grnLine.POLineItem.Product?.Name ?? "",
                                ProductSKU = grnLine.POLineItem.Product?.SKU ?? "",
                                WarehouseId = warehouseId,
                                QuantityReturned = lineItem.ReturnQuantity,
                                NewStockLevel = warehouseStock.Quantity
                            });
                        }
                    }
                }

                // === 9. UPDATE PO STATUSES ===
                await UpdatePurchaseOrderStatuses(purchaseOrder, cancellationToken);

                // === 10. FINAL SAVE ===
                await _refundRepo.SaveChanges();

                _logger.LogInformation("Successfully created refund: {RefundNumber}", refundNumber);

                // === 11. PREPARE RESPONSE DTO ===
                // Reload PO with fresh data for accurate response
                purchaseOrder = await _poRepo.GetAll()
                    .Include(po => po.LineItems)
                    .Include(po => po.Supplier)
                    .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId, cancellationToken);

                var refundDto = new RefundDto
                {
                    Id = refund.Id,
                    RefundNumber = refund.RefundNumber,
                    PurchaseOrderId = refund.PurchaseOrderId,
                    PurchaseOrderNumber = purchaseOrder.PONumber,
                    SupplierId = refund.SupplierId,
                    SupplierName = purchaseOrder.Supplier?.Name,
                    RefundDate = refund.RefundDate,
                    TotalAmount = refund.TotalAmount,
                    Reason = refund.Reason,
                    Notes = refund.Notes,
                    HasDebitNote = true,
                    DebitNote = new DebitNoteDto
                    {
                        Id = debitNote.Id,
                        DebitNoteNumber = debitNote.DebitNoteNumber,
                        RefundId = debitNote.RefundId,
                        SupplierId = debitNote.SupplierId,
                        SupplierName = purchaseOrder.Supplier?.Name ?? "",
                        NoteDate = debitNote.NoteDate,
                        Amount = debitNote.Amount,
                        Notes = debitNote.Notes
                    },
                    LineItems = refund.LineItems.Select(rl => new RefundLineItemDto
                    {
                        Id = rl.Id,
                        RefundId = rl.RefundId,
                        GRNLineItemId = rl.GRNLineItemId,
                        ProductId = grnLineItems.First(g => g.Id == rl.GRNLineItemId).POLineItem?.ProductId ?? Guid.Empty,
                        ProductName = grnLineItems.First(g => g.Id == rl.GRNLineItemId).POLineItem?.Product?.Name ?? "",
                        ProductSKU = grnLineItems.First(g => g.Id == rl.GRNLineItemId).POLineItem?.Product?.SKU ?? "",
                        ReturnQuantity = rl.ReturnQuantity,
                        UnitPrice = rl.UnitPrice,
                        LineTotal = rl.LineTotal
                    }).ToList(),
                    StatusUpdates = new StatusUpdatesDto
                    {
                        PurchaseOrder = new POStatusDto
                        {
                            ReceptionStatus = purchaseOrder.ReceptionStatus,
                            PaymentStatus = purchaseOrder.PaymentStatus,
                            DocumentStatus = purchaseOrder.DocumentStatus,
                            TotalOrdered = purchaseOrder.LineItems.Sum(l => l.Quantity),
                            TotalReceived = purchaseOrder.LineItems.Sum(l => l.ReceivedQuantity),
                            TotalReturned = purchaseOrder.LineItems.Sum(l => l.ReturnedQuantity),
                            NetReceived = purchaseOrder.LineItems.Sum(l => l.ReceivedQuantity - l.ReturnedQuantity)
                        },
                        Supplier = new SupplierStatusDto
                        {
                            SupplierId = purchaseOrder.SupplierId,
                            SupplierName = purchaseOrder.Supplier?.Name ?? "",
                            BalanceAdjusted = totalAmount,
                            NewBalance = 0 // يتم حسابه من الـ AP system
                        }
                    },
                    InventoryImpact = inventoryImpacts,
                    CreatedAt = refund.CreatedAt
                };

                return ResponseViewModel<RefundDto>.Success(refundDto, "Refund created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund for PO: {PurchaseOrderId}", request.PurchaseOrderId);
                throw;
            }
        }

        private async Task UpdatePurchaseOrderStatuses(PurchaseOrder purchaseOrder, CancellationToken cancellationToken)
        {
            // Recalculate from fresh data to ensure accuracy
            await _poRepo.GetAll()
                .Where(po => po.Id == purchaseOrder.Id)
                .Include(po => po.LineItems)
                .LoadAsync(cancellationToken);

            // Calculate totals
            var totalOrdered = purchaseOrder.LineItems.Sum(l => l.Quantity);
            var totalReceived = purchaseOrder.LineItems.Sum(l => l.ReceivedQuantity);
            var totalReturned = purchaseOrder.LineItems.Sum(l => l.ReturnedQuantity);
            var netReceived = totalReceived - totalReturned;

            // === UPDATE RECEPTION STATUS ===
            // Business Rule: Check the NET position (received - returned)
            if (netReceived <= 0 && totalReceived > 0)
            {
                // All received items were returned back
                purchaseOrder.ReceptionStatus = "Returned";
            }
            else if (totalReceived >= totalOrdered && netReceived > 0)
            {
                // Fully received (even if some were returned, net is still complete)
                purchaseOrder.ReceptionStatus = "FullyReceived";
            }
            else if (netReceived > 0 && netReceived < totalOrdered)
            {
                // Some items received but not all
                purchaseOrder.ReceptionStatus = "PartiallyReceived";
            }
            else if (totalReceived == 0)
            {
                // Nothing received yet
                purchaseOrder.ReceptionStatus = "NotReceived";
            }

            // === UPDATE PAYMENT STATUS ===
            // Calculate total amount due and total refunded
            var totalAmountDue = purchaseOrder.TotalAmount;

            // Get all payments (deposits + invoices payments)
            var totalPaid = purchaseOrder.DepositAmount;
            // TODO: Add sum of invoice payments when invoice module is integrated

            // Get total refunded amount
            var totalRefunded = await _refundRepo.GetAll()
                .Where(r => r.PurchaseOrderId == purchaseOrder.Id)
                .SumAsync(r => r.TotalAmount, cancellationToken);

            // Net payment position = paid - refunded
            var netPaymentBalance = totalPaid - totalRefunded;

            if (totalRefunded > 0 && netPaymentBalance <= 0 && totalPaid > 0)
            {
                // Supplier has refunded all or more than what was paid
                purchaseOrder.PaymentStatus = "Refunded";
            }
            else if (netPaymentBalance > 0 && netPaymentBalance >= totalAmountDue)
            {
                // Fully paid (even after refunds)
                purchaseOrder.PaymentStatus = "PaidInFull";
            }
            else if (netPaymentBalance > 0 && netPaymentBalance < totalAmountDue)
            {
                // Partially paid
                purchaseOrder.PaymentStatus = "PartiallyPaid";
            }
            else if (totalPaid == 0)
            {
                // Nothing paid yet
                purchaseOrder.PaymentStatus = "Unpaid";
            }

            // === CHECK IF PO CAN BE CLOSED ===
            // BRSD Rule: PO can only be closed if:
            // 1. Reception = FullyReceived or Returned
            // 2. Payment = PaidInFull or Refunded
            if ((purchaseOrder.ReceptionStatus == "FullyReceived" || purchaseOrder.ReceptionStatus == "Returned") &&
                (purchaseOrder.PaymentStatus == "PaidInFull" || purchaseOrder.PaymentStatus == "Refunded") &&
                purchaseOrder.DocumentStatus == "Approved")
            {
                // Auto-close criteria met (can be done via background job)
                _logger.LogInformation("PO {PONumber} is eligible for auto-closure", purchaseOrder.PONumber);
                // purchaseOrder.DocumentStatus = "Closed"; // Uncomment if you want auto-close
            }

            await _poRepo.Update(purchaseOrder);

            _logger.LogInformation(
                "Updated PO {PONumber} statuses - Reception: {Reception}, Payment: {Payment}, NetReceived: {NetReceived}/{TotalOrdered}, NetPayment: {NetPayment}/{TotalDue}",
                purchaseOrder.PONumber,
                purchaseOrder.ReceptionStatus,
                purchaseOrder.PaymentStatus,
                netReceived,
                totalOrdered,
                netPaymentBalance,
                totalAmountDue);
        }
    }
}
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_Refund
{
    public class UpdateRefundHandler : IRequestHandler<UpdateRefundCommand, ResponseViewModel<RefundDto>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IGeneralRepository<RefundLineItem> _lineItemRepo;
        private readonly IGeneralRepository<DebitNote> _debitNoteRepo;
        private readonly IGeneralRepository<PurchaseOrder> _poRepo;
        private readonly IGeneralRepository<GRNLineItem> _grnLineRepo;
        private readonly IGeneralRepository<POLineItem> _poLineRepo;
        private readonly IGeneralRepository<WarehouseStock> _stockRepo;
        private readonly IGeneralRepository<StockTransaction> _stockTransactionRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateRefundHandler> _logger;

        public UpdateRefundHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IGeneralRepository<RefundLineItem> lineItemRepo,
            IGeneralRepository<DebitNote> debitNoteRepo,
            IGeneralRepository<PurchaseOrder> poRepo,
            IGeneralRepository<GRNLineItem> grnLineRepo,
            IGeneralRepository<POLineItem> poLineRepo,
            IGeneralRepository<WarehouseStock> stockRepo,
            IGeneralRepository<StockTransaction> stockTransactionRepo,
            IMapper mapper,
            ILogger<UpdateRefundHandler> logger)
        {
            _refundRepo = refundRepo;
            _lineItemRepo = lineItemRepo;
            _debitNoteRepo = debitNoteRepo;
            _poRepo = poRepo;
            _grnLineRepo = grnLineRepo;
            _poLineRepo = poLineRepo;
            _stockRepo = stockRepo;
            _stockTransactionRepo = stockTransactionRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefundDto>> Handle(UpdateRefundCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating refund: {RefundId}", request.Id);

                // === 1. LOAD EXISTING REFUND ===
                var refund = await _refundRepo.GetAll()
                    .Include(r => r.LineItems)
                    .ThenInclude(rl => rl.GRNLineItem)
                    .ThenInclude(g => g.POLineItem)
                    .Include(r => r.DebitNote)
                    .Include(r => r.PurchaseOrder)
                    .ThenInclude(po => po.LineItems)
                    .Include(r => r.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                    .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

                if (refund == null)
                {
                    throw new NotFoundException(
                        $"Refund with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                // === 2. VALIDATION ===
                if (!request.LineItems.Any())
                {
                    throw new ValidationException(
                        "Refund must contain at least one line item",
                        new Dictionary<string, string[]> { { "LineItems", new[] { "At least one line item is required" } } },
                        "Purchases");
                }

                var purchaseOrder = refund.PurchaseOrder;

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
                    // Exclude current refund's quantities from the calculation
                    var alreadyReturned = await _refundRepo.GetAll()
                        .Where(r => r.PurchaseOrderId == refund.PurchaseOrderId && r.Id != refund.Id)
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

                // === 3. REVERSE OLD INVENTORY TRANSACTIONS ===
                var oldLineItems = refund.LineItems.ToList();

                foreach (var oldLine in oldLineItems)
                {
                    var grnLine = await _grnLineRepo.GetAll()
                        .Include(g => g.POLineItem)
                        .Include(g => g.GRN)
                        .FirstOrDefaultAsync(g => g.Id == oldLine.GRNLineItemId, cancellationToken);

                    if (grnLine?.POLineItem?.ProductId != null)
                    {
                        var warehouseId = grnLine.GRN.WarehouseId;
                        var productId = grnLine.POLineItem.ProductId.Value;

                        // Add back the returned quantity to stock
                        var warehouseStock = await _stockRepo.GetAll()
                            .FirstOrDefaultAsync(ws => ws.ProductId == productId && ws.WarehouseId == warehouseId, cancellationToken);

                        if (warehouseStock != null)
                        {
                            warehouseStock.Quantity += oldLine.ReturnQuantity;
                            warehouseStock.AvailableQuantity = warehouseStock.Quantity - (warehouseStock.ReservedQuantity ?? 0);
                            warehouseStock.TotalValue = warehouseStock.Quantity * (warehouseStock.AverageUnitCost ?? 0);
                            await _stockRepo.Update(warehouseStock);

                            // Create reversal stock transaction
                            var reversalTransaction = new StockTransaction
                            {
                                Id = Guid.NewGuid(),
                                CompanyId = purchaseOrder.CompanyId,
                                ProductId = productId,
                                WarehouseId = warehouseId,
                                TransactionType = StockTransactionType.Adjustment,
                                Quantity = oldLine.ReturnQuantity, // Positive to add back
                                UnitCost = oldLine.UnitPrice,
                                StockLevelAfter = warehouseStock.Quantity,
                                ReferenceType = "RefundUpdate_Reversal",
                                ReferenceId = refund.Id,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _stockTransactionRepo.AddAsync(reversalTransaction);
                        }
                    }

                    // Delete old line item
                    await _lineItemRepo.Delete(oldLine.Id);
                }

                // === 4. UPDATE BASIC FIELDS ===
                refund.RefundDate = request.RefundDate;
                refund.Reason = request.Reason;
                refund.Notes = request.Notes;
                refund.UpdatedAt = DateTime.UtcNow;

                // === 5. CALCULATE NEW TOTAL ===
                var totalAmount = request.LineItems.Sum(item => item.ReturnQuantity * item.UnitPrice);
                refund.TotalAmount = totalAmount;

                // === 6. CREATE NEW LINE ITEMS ===
                var inventoryImpacts = new List<InventoryImpactDto>();

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
                    await _lineItemRepo.AddAsync(refundLine);
                }

                await _refundRepo.SaveChanges();

                // === 7. UPDATE INVENTORY WITH NEW QUANTITIES ===
                foreach (var lineItem in request.LineItems)
                {
                    var grnLine = grnLineItems.First(g => g.Id == lineItem.GRNLineItemId);

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
                                ReferenceType = "PurchaseRefund_Update",
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

                // === 8. RECALCULATE PO LINE ITEMS ===
                var affectedPOLineItems = grnLineItems
                    .Select(g => g.POLineItemId)
                    .Distinct()
                    .ToList();

                foreach (var poLineItemId in affectedPOLineItems)
                {
                    var poLineItem = await _poLineRepo.GetAll()
                        .Include(pol => pol.RefundLineItems)
                        .ThenInclude(rl => rl.GRNLineItem)
                        .FirstOrDefaultAsync(pol => pol.Id == poLineItemId, cancellationToken);

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
                            "Recalculated POLineItem {POLineItemId}: Returned={Returned}",
                            poLineItem.Id,
                            poLineItem.ReturnedQuantity);
                    }
                }

                // === 9. UPDATE DEBIT NOTE ===
                if (refund.DebitNote != null)
                {
                    var debitNote = await _debitNoteRepo.GetByIDWithTracking(refund.DebitNote.Id);
                    if (debitNote != null)
                    {
                        debitNote.Amount = totalAmount;
                        debitNote.Notes = $"Updated - Auto-generated for Refund: {refund.RefundNumber}";
                        await _debitNoteRepo.Update(debitNote);
                        _logger.LogInformation("Updated Debit Note: {DebitNoteNumber}", debitNote.DebitNoteNumber);
                    }
                }

                // === 10. UPDATE PO STATUSES ===
                await UpdatePurchaseOrderStatuses(purchaseOrder, cancellationToken);

                // === 11. FINAL SAVE ===
                await _refundRepo.SaveChanges();

                _logger.LogInformation("Successfully updated refund: {RefundId}", request.Id);

                // === 12. PREPARE RESPONSE DTO ===
                // Reload with fresh data
                purchaseOrder = await _poRepo.GetAll()
                    .Include(po => po.LineItems)
                    .Include(po => po.Supplier)
                    .FirstOrDefaultAsync(po => po.Id == refund.PurchaseOrderId, cancellationToken);

                var debitNoteDto = refund.DebitNote != null ? new DebitNoteDto
                {
                    Id = refund.DebitNote.Id,
                    DebitNoteNumber = refund.DebitNote.DebitNoteNumber,
                    RefundId = refund.DebitNote.RefundId,
                    SupplierId = refund.DebitNote.SupplierId,
                    SupplierName = purchaseOrder.Supplier?.Name ?? "",
                    NoteDate = refund.DebitNote.NoteDate,
                    Amount = refund.DebitNote.Amount,
                    Notes = refund.DebitNote.Notes
                } : null;

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
                    HasDebitNote = refund.DebitNote != null,
                    DebitNote = debitNoteDto,
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
                            NewBalance = 0
                        }
                    },
                    InventoryImpact = inventoryImpacts,
                    CreatedAt = refund.CreatedAt
                };

                return ResponseViewModel<RefundDto>.Success(refundDto, "Refund updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund: {RefundId}", request.Id);
                throw;
            }
        }

        private async Task UpdatePurchaseOrderStatuses(PurchaseOrder purchaseOrder, CancellationToken cancellationToken)
        {
            // Reload fresh data
            await _poRepo.GetAll()
                .Where(po => po.Id == purchaseOrder.Id)
                .Include(po => po.LineItems)
                .LoadAsync(cancellationToken);

            var totalOrdered = purchaseOrder.LineItems.Sum(l => l.Quantity);
            var totalReceived = purchaseOrder.LineItems.Sum(l => l.ReceivedQuantity);
            var totalReturned = purchaseOrder.LineItems.Sum(l => l.ReturnedQuantity);
            var netReceived = totalReceived - totalReturned;

            // Update Reception Status
            if (netReceived <= 0 && totalReceived > 0)
            {
                purchaseOrder.ReceptionStatus = "Returned";
            }
            else if (totalReceived >= totalOrdered && netReceived > 0)
            {
                purchaseOrder.ReceptionStatus = "FullyReceived";
            }
            else if (netReceived > 0 && netReceived < totalOrdered)
            {
                purchaseOrder.ReceptionStatus = "PartiallyReceived";
            }
            else if (totalReceived == 0)
            {
                purchaseOrder.ReceptionStatus = "NotReceived";
            }

            // Update Payment Status
            var totalAmountDue = purchaseOrder.TotalAmount;
            var totalPaid = purchaseOrder.DepositAmount;

            var totalRefunded = await _refundRepo.GetAll()
                .Where(r => r.PurchaseOrderId == purchaseOrder.Id)
                .SumAsync(r => r.TotalAmount, cancellationToken);

            var netPaymentBalance = totalPaid - totalRefunded;

            if (totalRefunded > 0 && netPaymentBalance <= 0 && totalPaid > 0)
            {
                purchaseOrder.PaymentStatus = "Refunded";
            }
            else if (netPaymentBalance > 0 && netPaymentBalance >= totalAmountDue)
            {
                purchaseOrder.PaymentStatus = "PaidInFull";
            }
            else if (netPaymentBalance > 0 && netPaymentBalance < totalAmountDue)
            {
                purchaseOrder.PaymentStatus = "PartiallyPaid";
            }
            else if (totalPaid == 0)
            {
                purchaseOrder.PaymentStatus = "Unpaid";
            }

            await _poRepo.Update(purchaseOrder);

            _logger.LogInformation(
                "Updated PO {PONumber} statuses after refund update - Reception: {Reception}, Payment: {Payment}",
                purchaseOrder.PONumber,
                purchaseOrder.ReceptionStatus,
                purchaseOrder.PaymentStatus);
        }
    }
}
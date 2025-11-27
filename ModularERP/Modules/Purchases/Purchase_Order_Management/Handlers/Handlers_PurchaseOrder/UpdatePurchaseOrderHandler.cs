using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_PurchaseOrder
{
    public class UpdatePurchaseOrderHandler : IRequestHandler<UpdatePurchaseOrderCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IGeneralRepository<POLineItem> _lineItemRepository;
        private readonly IGeneralRepository<PODeposit> _depositRepository;
        private readonly IGeneralRepository<POShippingCharge> _shippingRepository;
        private readonly IGeneralRepository<PODiscount> _discountRepository;
        private readonly IGeneralRepository<POAdjustment> _adjustmentRepository;
        private readonly IGeneralRepository<POAttachment> _attachmentRepository;
        private readonly IGeneralRepository<PONote> _noteRepository;
        private readonly IGeneralRepository<PaymentTerm> _paymentTermRepository;
        private readonly IGeneralRepository<PaymentMethod> _paymentMethodRepository; // ✅ Added
        private readonly IMapper _mapper;
        private readonly ILogger<UpdatePurchaseOrderHandler> _logger;
        private readonly IWebHostEnvironment _environment;

        public UpdatePurchaseOrderHandler(
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<POLineItem> lineItemRepository,
            IGeneralRepository<PODeposit> depositRepository,
            IGeneralRepository<POShippingCharge> shippingRepository,
            IGeneralRepository<PODiscount> discountRepository,
            IGeneralRepository<POAdjustment> adjustmentRepository,
            IGeneralRepository<POAttachment> attachmentRepository,
            IGeneralRepository<PONote> noteRepository,
            IGeneralRepository<PaymentTerm> paymentTermRepository,
            IGeneralRepository<PaymentMethod> paymentMethodRepository, // ✅ Added
            IMapper mapper,
            ILogger<UpdatePurchaseOrderHandler> logger,
            IWebHostEnvironment environment)
        {
            _poRepository = poRepository;
            _lineItemRepository = lineItemRepository;
            _depositRepository = depositRepository;
            _shippingRepository = shippingRepository;
            _discountRepository = discountRepository;
            _adjustmentRepository = adjustmentRepository;
            _attachmentRepository = attachmentRepository;
            _noteRepository = noteRepository;
            _paymentTermRepository = paymentTermRepository;
            _paymentMethodRepository = paymentMethodRepository; // ✅ Added
            _mapper = mapper;
            _environment = environment;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            UpdatePurchaseOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating purchase order with ID: {POId}", request.Id);

                // Get existing PO
                var purchaseOrder = await _poRepository.GetByIDWithTracking(request.Id);
                if (purchaseOrder == null)
                {
                    throw new NotFoundException(
                        $"Purchase order with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Found PO: {PONumber}, Status: {Status}",
                    purchaseOrder.PONumber, purchaseOrder.DocumentStatus);

                // Validate status - only Draft can be edited
                if (purchaseOrder.DocumentStatus != DocumentStatus.Draft)
                {
                    throw new BusinessLogicException(
                        "Only purchase orders in Draft status can be updated",
                        "PurchaseOrder");
                }

                // ✅ Validate PaymentTermId if provided
                if (request.PaymentTermId.HasValue)
                {
                    var paymentTerm = await _paymentTermRepository.GetByID(request.PaymentTermId.Value);
                    if (paymentTerm == null)
                    {
                        throw new NotFoundException(
                            $"Payment term with ID {request.PaymentTermId.Value} not found",
                            FinanceErrorCode.NotFound);
                    }
                    _logger.LogInformation("Validated payment term: {PaymentTermName}", paymentTerm.Name);
                }

                // ✅ Validate PaymentMethodIds in deposits
                if (request.Deposits?.Any() == true)
                {
                    var paymentMethodIds = request.Deposits.Select(d => d.PaymentMethodId).Distinct().ToList();
                    var existingPaymentMethods = await _paymentMethodRepository.GetAll()
                        .Where(pm => paymentMethodIds.Contains(pm.Id))
                        .Select(pm => pm.Id)
                        .ToListAsync(cancellationToken);

                    var missingPaymentMethods = paymentMethodIds.Except(existingPaymentMethods).ToList();
                    if (missingPaymentMethods.Any())
                    {
                        throw new NotFoundException(
                            $"Payment methods not found: {string.Join(", ", missingPaymentMethods)}",
                            FinanceErrorCode.NotFound);
                    }

                    _logger.LogInformation("Validated {Count} payment methods", existingPaymentMethods.Count);
                }

                // Update main PO fields
                purchaseOrder.SupplierId = request.SupplierId;
                purchaseOrder.CurrencyCode = request.CurrencyCode;
                purchaseOrder.PODate = request.PODate;
                purchaseOrder.PaymentTermId = request.PaymentTermId;
                purchaseOrder.Notes = request.Notes;
                purchaseOrder.Terms = request.Terms;
                purchaseOrder.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Updated main PO fields");

                // Update line items
                await UpdateLineItems(purchaseOrder.Id, request.LineItems, cancellationToken);

                // Update deposits
                await UpdateDeposits(purchaseOrder.Id, request.Deposits, cancellationToken);

                // Update shipping charges
                await UpdateShippingCharges(purchaseOrder.Id, request.ShippingCharges, cancellationToken);

                // Update discounts
                await UpdateDiscounts(purchaseOrder.Id, request.Discounts, cancellationToken);

                // Update adjustments
                await UpdateAdjustments(purchaseOrder.Id, request.Adjustments, cancellationToken);

                // ✅ Recalculate amounts
                var calculations = await RecalculateAmounts(purchaseOrder.Id, cancellationToken);
                purchaseOrder.Subtotal = calculations.Subtotal;
                purchaseOrder.DiscountAmount = calculations.DiscountAmount;
                purchaseOrder.AdjustmentAmount = calculations.AdjustmentAmount;
                purchaseOrder.ShippingAmount = calculations.ShippingAmount;
                purchaseOrder.TaxAmount = calculations.TaxAmount;
                purchaseOrder.TotalAmount = calculations.TotalAmount;
                purchaseOrder.DepositAmount = calculations.DepositAmount;
                purchaseOrder.AmountDue = calculations.AmountDue;

                // Update payment status
                purchaseOrder.PaymentStatus = calculations.DepositAmount > 0
                    ? PaymentStatus.PartiallyPaid
                    : PaymentStatus.Unpaid;

                _logger.LogInformation(
                    "Recalculated amounts - Total: {Total}, Deposit: {Deposit}, Due: {Due}",
                    calculations.TotalAmount, calculations.DepositAmount, calculations.AmountDue);

                // Handle attachments
                if (request.AttachmentsToDelete?.Any() == true)
                    await DeleteAttachments(request.AttachmentsToDelete, cancellationToken);

                if (request.NewAttachments?.Any() == true)
                    await AddNewAttachments(purchaseOrder.Id, request.NewAttachments, cancellationToken);

                // Add new notes
                if (request.NewNotes?.Any() == true)
                    await AddNewNotes(purchaseOrder.Id, request.NewNotes, cancellationToken);

                await _poRepository.SaveChanges();

                _logger.LogInformation("Purchase order {PONumber} updated successfully", purchaseOrder.PONumber);

                return ResponseViewModel<bool>.Success(true, "Purchase order updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase order with ID: {POId}", request.Id);
                throw;
            }
        }

        /// <summary>
        /// Recalculates amounts using the same logic as CreatePurchaseOrderHandler
        /// </summary>
        private async Task<(decimal Subtotal, decimal DiscountAmount, decimal AdjustmentAmount,
                           decimal ShippingAmount, decimal TaxAmount, decimal TotalAmount,
                           decimal DepositAmount, decimal AmountDue)> RecalculateAmounts(
            Guid purchaseOrderId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recalculating amounts for PO: {POId}", purchaseOrderId);

            // Fetch all related data
            var lineItems = await _lineItemRepository
                .Get(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);

            var deposits = await _depositRepository
                .Get(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);

            var shippingCharges = await _shippingRepository
                .Get(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);

            var discounts = await _discountRepository
                .Get(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);

            var adjustments = await _adjustmentRepository
                .Get(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);

            // STEP 1: Line Items Totals
            decimal lineItemsGrossTotal = 0;
            decimal lineItemsNetTotal = 0;
            decimal lineItemsTaxTotal = 0;

            foreach (var item in lineItems)
            {
                var lineGrossAmount = item.Quantity * item.UnitPrice;
                lineItemsGrossTotal += lineGrossAmount;

                var lineNetAmount = lineGrossAmount - item.DiscountAmount;
                lineItemsNetTotal += lineNetAmount;

                lineItemsTaxTotal += item.TaxAmount;
            }

            _logger.LogInformation(
                "Line Items Summary - Gross: {Gross}, Net: {Net}, Tax: {Tax}",
                lineItemsGrossTotal, lineItemsNetTotal, lineItemsTaxTotal);

            // STEP 2: PO-Level Discounts
            decimal poDiscountAmount = 0;

            foreach (var discount in discounts)
            {
                if (discount.DiscountType == DiscountType.Percentage)
                {
                    poDiscountAmount += lineItemsNetTotal * (discount.DiscountValue / 100);
                }
                else
                {
                    poDiscountAmount += discount.DiscountValue;
                }
            }

            decimal netAfterPODiscount = lineItemsNetTotal - poDiscountAmount;

            _logger.LogInformation(
                "After PO Discount - Discount: {Discount}, Net: {Net}",
                poDiscountAmount, netAfterPODiscount);

            // STEP 3: Adjustments
            decimal adjustmentAmount = adjustments.Sum(x => x.Amount);

            // STEP 4: Shipping
            decimal shippingFeeTotal = shippingCharges.Sum(x => x.ShippingFee);
            decimal shippingTaxTotal = shippingCharges.Sum(x => x.TaxAmount);

            // STEP 5: Grand Total
            decimal totalTax = lineItemsTaxTotal + shippingTaxTotal;
            decimal grandTotal = netAfterPODiscount + adjustmentAmount + shippingFeeTotal + totalTax;

            _logger.LogInformation(
                "Grand Total Calculation: Net={Net} + Adj={Adj} + Ship={Ship} + Tax={Tax} = {Total}",
                netAfterPODiscount, adjustmentAmount, shippingFeeTotal, totalTax, grandTotal);

            // STEP 6: Deposits
            decimal depositAmount = 0;

            foreach (var deposit in deposits)
            {
                if (deposit.Percentage.HasValue && deposit.Percentage.Value > 0)
                {
                    var calculatedDeposit = grandTotal * (deposit.Percentage.Value / 100);
                    depositAmount += calculatedDeposit;

                    _logger.LogDebug(
                        "Deposit: {Percent}% of {Total} = {Amount}",
                        deposit.Percentage.Value, grandTotal, calculatedDeposit);
                }
                else if (deposit.Amount > 0)
                {
                    depositAmount += deposit.Amount;

                    _logger.LogDebug("Deposit: Fixed Amount = {Amount}", deposit.Amount);
                }
            }

            if (depositAmount > grandTotal)
            {
                _logger.LogWarning(
                    "Deposit ({Deposit}) exceeds Grand Total ({Total}). Capping to total amount.",
                    depositAmount, grandTotal);
                depositAmount = grandTotal;
            }

            // STEP 7: Amount Due
            decimal amountDue = Math.Max(0, grandTotal - depositAmount);

            _logger.LogInformation(
                "✅ Recalculation Complete - Subtotal: {Subtotal}, Discount: {Discount}, " +
                "Adjustment: {Adjustment}, Shipping: {Shipping}, Tax: {Tax}, " +
                "Total: {Total}, Deposit: {Deposit}, Due: {Due}",
                lineItemsGrossTotal, poDiscountAmount, adjustmentAmount,
                shippingFeeTotal, totalTax, grandTotal, depositAmount, amountDue);

            return (
                Subtotal: lineItemsGrossTotal,
                DiscountAmount: poDiscountAmount,
                AdjustmentAmount: adjustmentAmount,
                ShippingAmount: shippingFeeTotal,
                TaxAmount: totalTax,
                TotalAmount: grandTotal,
                DepositAmount: depositAmount,
                AmountDue: amountDue
            );
        }

        private async Task UpdateLineItems(Guid purchaseOrderId, List<UpdatePOLineItemDto> items,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating line items for PO: {POId}", purchaseOrderId);

            var existingItems = await _lineItemRepository
                .Get(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);

            // Delete removed items
            var itemsToDelete = existingItems.Where(e => !items.Any(i => i.Id == e.Id)).ToList();
            foreach (var item in itemsToDelete)
            {
                _logger.LogDebug("Deleting line item: {ItemId}", item.Id);
                await _lineItemRepository.Delete(item.Id);
            }

            // Update or add items
            foreach (var itemDto in items)
            {
                var lineAmount = itemDto.Quantity * itemDto.UnitPrice;
                var discountAmount = lineAmount * (itemDto.DiscountPercent / 100);
                var netAmount = lineAmount - discountAmount;
                var taxAmount = itemDto.TaxProfileId.HasValue ? netAmount * 0.15m : 0;
                var lineTotal = netAmount + taxAmount;

                if (itemDto.Id.HasValue && itemDto.Id.Value != Guid.Empty)
                {
                    var existing = existingItems.FirstOrDefault(x => x.Id == itemDto.Id.Value);
                    if (existing != null)
                    {
                        _logger.LogDebug("Updating line item: {ItemId}", existing.Id);

                        existing.ProductId = itemDto.ProductId;
                        existing.ServiceId = itemDto.ServiceId;
                        existing.Description = itemDto.Description;
                        existing.Quantity = itemDto.Quantity;
                        existing.UnitPrice = itemDto.UnitPrice;
                        existing.DiscountPercent = itemDto.DiscountPercent;
                        existing.DiscountAmount = discountAmount;
                        existing.TaxProfileId = itemDto.TaxProfileId;
                        existing.TaxAmount = taxAmount;
                        existing.LineTotal = lineTotal;
                        existing.RemainingQuantity = itemDto.Quantity - existing.ReceivedQuantity;

                        await _lineItemRepository.Update(existing);
                    }
                }
                else
                {
                    _logger.LogDebug("Adding new line item");

                    var newItem = new POLineItem
                    {
                        Id = Guid.NewGuid(),
                        PurchaseOrderId = purchaseOrderId,
                        ProductId = itemDto.ProductId,
                        ServiceId = itemDto.ServiceId,
                        Description = itemDto.Description,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        DiscountPercent = itemDto.DiscountPercent,
                        DiscountAmount = discountAmount,
                        TaxProfileId = itemDto.TaxProfileId,
                        TaxAmount = taxAmount,
                        LineTotal = lineTotal,
                        RemainingQuantity = itemDto.Quantity
                    };

                    await _lineItemRepository.AddAsync(newItem);
                }
            }

            await _lineItemRepository.SaveChanges();
            _logger.LogInformation("Line items updated successfully");
        }

        private async Task UpdateDeposits(Guid purchaseOrderId, List<UpdatePODepositDto> deposits,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating deposits for PO: {POId}", purchaseOrderId);

            var existingDeposits = await _depositRepository
                .Get(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);

            // Delete removed deposits
            var depositsToDelete = existingDeposits.Where(e => !deposits.Any(d => d.Id == e.Id)).ToList();
            foreach (var deposit in depositsToDelete)
            {
                _logger.LogDebug("Deleting deposit: {DepositId}", deposit.Id);
                await _depositRepository.Delete(deposit.Id);
            }

            // Update or add deposits
            foreach (var depositDto in deposits)
            {
                if (depositDto.Id.HasValue && depositDto.Id.Value != Guid.Empty)
                {
                    var existing = existingDeposits.FirstOrDefault(x => x.Id == depositDto.Id.Value);
                    if (existing != null)
                    {
                        _logger.LogDebug("Updating deposit: {DepositId}", existing.Id);

                        existing.Amount = depositDto.Amount;
                        existing.Percentage = depositDto.Percentage;
                        existing.PaymentMethodId = depositDto.PaymentMethodId;
                        existing.ReferenceNumber = depositDto.ReferenceNumber;
                        existing.AlreadyPaid = depositDto.AlreadyPaid;
                        existing.PaymentDate = depositDto.PaymentDate;
                        existing.Notes = depositDto.Notes;

                        await _depositRepository.Update(existing);
                    }
                }
                else
                {
                    _logger.LogDebug("Adding new deposit with PaymentMethodId: {PaymentMethodId}",
                        depositDto.PaymentMethodId);

                    await _depositRepository.AddAsync(new PODeposit
                    {
                        Id = Guid.NewGuid(),
                        PurchaseOrderId = purchaseOrderId,
                        Amount = depositDto.Amount,
                        Percentage = depositDto.Percentage,
                        PaymentMethodId = depositDto.PaymentMethodId,
                        ReferenceNumber = depositDto.ReferenceNumber,
                        AlreadyPaid = depositDto.AlreadyPaid,
                        PaymentDate = depositDto.PaymentDate,
                        Notes = depositDto.Notes
                    });
                }
            }

            await _depositRepository.SaveChanges();
            _logger.LogInformation("Deposits updated successfully");
        }

        private async Task UpdateShippingCharges(Guid purchaseOrderId, List<UpdatePOShippingChargeDto> charges,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating shipping charges for PO: {POId}", purchaseOrderId);

            var existingCharges = await _shippingRepository
                .Get(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);

            var chargesToDelete = existingCharges.Where(e => !charges.Any(c => c.Id == e.Id)).ToList();
            foreach (var charge in chargesToDelete)
            {
                await _shippingRepository.Delete(charge.Id);
            }

            foreach (var chargeDto in charges)
            {
                var taxAmount = chargeDto.TaxProfileId.HasValue ? chargeDto.ShippingFee * 0.15m : 0;

                if (chargeDto.Id.HasValue && chargeDto.Id.Value != Guid.Empty)
                {
                    var existing = existingCharges.FirstOrDefault(x => x.Id == chargeDto.Id.Value);
                    if (existing != null)
                    {
                        existing.ShippingFee = chargeDto.ShippingFee;
                        existing.TaxProfileId = chargeDto.TaxProfileId;
                        existing.TaxAmount = taxAmount;
                        existing.Total = chargeDto.ShippingFee + taxAmount;
                        existing.Description = chargeDto.Description;

                        await _shippingRepository.Update(existing);
                    }
                }
                else
                {
                    await _shippingRepository.AddAsync(new POShippingCharge
                    {
                        Id = Guid.NewGuid(),
                        PurchaseOrderId = purchaseOrderId,
                        ShippingFee = chargeDto.ShippingFee,
                        TaxProfileId = chargeDto.TaxProfileId,
                        TaxAmount = taxAmount,
                        Total = chargeDto.ShippingFee + taxAmount,
                        Description = chargeDto.Description
                    });
                }
            }

            await _shippingRepository.SaveChanges();
        }

        private async Task UpdateDiscounts(Guid purchaseOrderId, List<UpdatePODiscountDto> discounts,
            CancellationToken cancellationToken)
        {
            var existingDiscounts = await _discountRepository
                .Get(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);

            var discountsToDelete = existingDiscounts.Where(e => !discounts.Any(d => d.Id == e.Id)).ToList();
            foreach (var discount in discountsToDelete)
            {
                await _discountRepository.Delete(discount.Id);
            }

            foreach (var discountDto in discounts)
            {
                if (discountDto.Id.HasValue && discountDto.Id.Value != Guid.Empty)
                {
                    var existing = existingDiscounts.FirstOrDefault(x => x.Id == discountDto.Id.Value);
                    if (existing != null)
                    {
                        existing.DiscountType = Enum.Parse<DiscountType>(discountDto.DiscountType);
                        existing.DiscountValue = discountDto.DiscountValue;
                        existing.DiscountAmount = discountDto.DiscountValue;
                        existing.Description = discountDto.Description;

                        await _discountRepository.Update(existing);
                    }
                }
                else
                {
                    await _discountRepository.AddAsync(new PODiscount
                    {
                        Id = Guid.NewGuid(),
                        PurchaseOrderId = purchaseOrderId,
                        DiscountType = Enum.Parse<DiscountType>(discountDto.DiscountType),
                        DiscountValue = discountDto.DiscountValue,
                        DiscountAmount = discountDto.DiscountValue,
                        Description = discountDto.Description
                    });
                }
            }

            await _discountRepository.SaveChanges();
        }

        private async Task UpdateAdjustments(Guid purchaseOrderId, List<UpdatePOAdjustmentDto> adjustments,
            CancellationToken cancellationToken)
        {
            var existingAdjustments = await _adjustmentRepository
                .Get(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);

            var adjustmentsToDelete = existingAdjustments.Where(e => !adjustments.Any(a => a.Id == e.Id)).ToList();
            foreach (var adjustment in adjustmentsToDelete)
            {
                await _adjustmentRepository.Delete(adjustment.Id);
            }

            foreach (var adjustmentDto in adjustments)
            {
                if (adjustmentDto.Id.HasValue && adjustmentDto.Id.Value != Guid.Empty)
                {
                    var existing = existingAdjustments.FirstOrDefault(x => x.Id == adjustmentDto.Id.Value);
                    if (existing != null)
                    {
                        existing.AdjustmentType = adjustmentDto.AdjustmentType;
                        existing.Amount = adjustmentDto.Amount;
                        existing.Notes = adjustmentDto.Notes;

                        await _adjustmentRepository.Update(existing);
                    }
                }
                else
                {
                    await _adjustmentRepository.AddAsync(new POAdjustment
                    {
                        Id = Guid.NewGuid(),
                        PurchaseOrderId = purchaseOrderId,
                        AdjustmentType = adjustmentDto.AdjustmentType,
                        Amount = adjustmentDto.Amount,
                        Notes = adjustmentDto.Notes
                    });
                }
            }

            await _adjustmentRepository.SaveChanges();
        }

        private async Task DeleteAttachments(List<Guid> attachmentIds, CancellationToken cancellationToken)
        {
            foreach (var attachmentId in attachmentIds)
            {
                var attachment = await _attachmentRepository.GetByID(attachmentId);
                if (attachment != null)
                {
                    if (File.Exists(attachment.FilePath))
                    {
                        File.Delete(attachment.FilePath);
                    }

                    await _attachmentRepository.Delete(attachmentId);
                }
            }

            await _attachmentRepository.SaveChanges();
        }

        private async Task AddNewAttachments(Guid purchaseOrderId, List<IFormFile> attachments,
            CancellationToken cancellationToken)
        {
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "purchase-orders", purchaseOrderId.ToString());
            Directory.CreateDirectory(uploadPath);

            var attachmentEntities = new List<POAttachment>();

            foreach (var file in attachments)
            {
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                attachmentEntities.Add(new POAttachment
                {
                    Id = Guid.NewGuid(),
                    PurchaseOrderId = purchaseOrderId,
                    FileName = file.FileName,
                    FilePath = filePath,
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    UploadedAt = DateTime.UtcNow,
                    UploadedBy = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53")
                });
            }

            await _attachmentRepository.AddRangeAsync(attachmentEntities);
            await _attachmentRepository.SaveChanges();
        }

        private async Task AddNewNotes(Guid purchaseOrderId, List<string> notes,
            CancellationToken cancellationToken)
        {
            var noteEntities = notes.Select(n => new PONote
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = purchaseOrderId,
                NoteText = n,
                CreatedAt = DateTime.UtcNow,
                CreatedById = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53")
            }).ToList();

            await _noteRepository.AddRangeAsync(noteEntities);
            await _noteRepository.SaveChanges();
        }
    }
}
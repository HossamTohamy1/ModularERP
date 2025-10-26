using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;
using Serilog;

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

                // Validate status - only Draft can be edited
                if (purchaseOrder.DocumentStatus != "Draft")
                {
                    throw new BusinessLogicException(
                        "Only purchase orders in Draft status can be updated",
                        "PurchaseOrder");
                }

                // Update main PO fields
                purchaseOrder.SupplierId = request.SupplierId;
                purchaseOrder.CurrencyCode = request.CurrencyCode;
                purchaseOrder.PODate = request.PODate;
                purchaseOrder.PaymentTerms = request.PaymentTerms;
                purchaseOrder.Notes = request.Notes;
                purchaseOrder.Terms = request.Terms;
                purchaseOrder.UpdatedAt = DateTime.UtcNow;

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

                // Recalculate amounts
                var calculations = await RecalculateAmounts(purchaseOrder.Id, cancellationToken);
                purchaseOrder.Subtotal = calculations.Subtotal;
                purchaseOrder.DiscountAmount = calculations.DiscountAmount;
                purchaseOrder.AdjustmentAmount = calculations.AdjustmentAmount;
                purchaseOrder.ShippingAmount = calculations.ShippingAmount;
                purchaseOrder.TaxAmount = calculations.TaxAmount;
                purchaseOrder.TotalAmount = calculations.TotalAmount;
                purchaseOrder.DepositAmount = calculations.DepositAmount;
                purchaseOrder.AmountDue = calculations.AmountDue;

                // Update payment status based on deposits
                purchaseOrder.PaymentStatus = calculations.DepositAmount > 0 ? "PartiallyPaid" : "Unpaid";

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

        private async Task UpdateLineItems(Guid purchaseOrderId, List<UpdatePOLineItemDto> items,
            CancellationToken cancellationToken)
        {
            // Get existing line items
            var existingItems = await _lineItemRepository
                .Get(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);

            // Delete items not in the update list
            var itemsToDelete = existingItems.Where(e => !items.Any(i => i.Id == e.Id)).ToList();
            foreach (var item in itemsToDelete)
            {
                await _lineItemRepository.Delete(item.Id);
            }

            // Update or add items
            foreach (var itemDto in items)
            {
                if (itemDto.Id.HasValue && itemDto.Id.Value != Guid.Empty)
                {
                    // Update existing
                    var existing = existingItems.FirstOrDefault(x => x.Id == itemDto.Id.Value);
                    if (existing != null)
                    {
                        var lineAmount = itemDto.Quantity * itemDto.UnitPrice;
                        var discountAmount = lineAmount * (itemDto.DiscountPercent / 100);
                        var netAmount = lineAmount - discountAmount;
                        var taxAmount = itemDto.TaxProfileId.HasValue ? netAmount * 0.15m : 0;

                        existing.ProductId = itemDto.ProductId;
                        existing.ServiceId = itemDto.ServiceId;
                        existing.Description = itemDto.Description;
                        existing.Quantity = itemDto.Quantity;
                        existing.UnitPrice = itemDto.UnitPrice;
                        existing.DiscountPercent = itemDto.DiscountPercent;
                        existing.DiscountAmount = discountAmount;
                        existing.TaxProfileId = itemDto.TaxProfileId;
                        existing.TaxAmount = taxAmount;
                        existing.LineTotal = netAmount + taxAmount;
                        existing.RemainingQuantity = itemDto.Quantity - existing.ReceivedQuantity;

                        await _lineItemRepository.Update(existing);
                    }
                }
                else
                {
                    // Add new
                    var lineAmount = itemDto.Quantity * itemDto.UnitPrice;
                    var discountAmount = lineAmount * (itemDto.DiscountPercent / 100);
                    var netAmount = lineAmount - discountAmount;
                    var taxAmount = itemDto.TaxProfileId.HasValue ? netAmount * 0.15m : 0;

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
                        LineTotal = netAmount + taxAmount,
                        RemainingQuantity = itemDto.Quantity
                    };

                    await _lineItemRepository.AddAsync(newItem);
                }
            }

            await _lineItemRepository.SaveChanges();
        }

        private async Task UpdateDeposits(Guid purchaseOrderId, List<UpdatePODepositDto> deposits,
            CancellationToken cancellationToken)
        {
            var existingDeposits = await _depositRepository
                .Get(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);

            var depositsToDelete = existingDeposits.Where(e => !deposits.Any(d => d.Id == e.Id)).ToList();
            foreach (var deposit in depositsToDelete)
            {
                await _depositRepository.Delete(deposit.Id);
            }

            foreach (var depositDto in deposits)
            {
                if (depositDto.Id.HasValue && depositDto.Id.Value != Guid.Empty)
                {
                    var existing = existingDeposits.FirstOrDefault(x => x.Id == depositDto.Id.Value);
                    if (existing != null)
                    {
                        existing.Amount = depositDto.Amount;
                        existing.Percentage = depositDto.Percentage;
                        existing.PaymentMethod = depositDto.PaymentMethod;
                        existing.ReferenceNumber = depositDto.ReferenceNumber;
                        existing.AlreadyPaid = depositDto.AlreadyPaid;
                        existing.PaymentDate = depositDto.PaymentDate;
                        existing.Notes = depositDto.Notes;

                        await _depositRepository.Update(existing);
                    }
                }
                else
                {
                    await _depositRepository.AddAsync(new PODeposit
                    {
                        Id = Guid.NewGuid(),
                        PurchaseOrderId = purchaseOrderId,
                        Amount = depositDto.Amount,
                        Percentage = depositDto.Percentage,
                        PaymentMethod = depositDto.PaymentMethod,
                        ReferenceNumber = depositDto.ReferenceNumber,
                        AlreadyPaid = depositDto.AlreadyPaid,
                        PaymentDate = depositDto.PaymentDate,
                        Notes = depositDto.Notes
                    });
                }
            }

            await _depositRepository.SaveChanges();
        }

        private async Task UpdateShippingCharges(Guid purchaseOrderId, List<UpdatePOShippingChargeDto> charges,
            CancellationToken cancellationToken)
        {
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
                        existing.DiscountType = discountDto.DiscountType;
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
                        DiscountType = discountDto.DiscountType,
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

        private async Task<(decimal Subtotal, decimal DiscountAmount, decimal AdjustmentAmount,
                           decimal ShippingAmount, decimal TaxAmount, decimal TotalAmount,
                           decimal DepositAmount, decimal AmountDue)> RecalculateAmounts(
            Guid purchaseOrderId, CancellationToken cancellationToken)
        {
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

            var subtotal = lineItems.Sum(x => x.Quantity * x.UnitPrice);
            var taxAmount = lineItems.Sum(x => x.TaxAmount) + shippingCharges.Sum(x => x.TaxAmount);
            var discountAmount = discounts.Sum(x => x.DiscountAmount);
            var adjustmentAmount = adjustments.Sum(x => x.Amount);
            var shippingAmount = shippingCharges.Sum(x => x.ShippingFee);
            var depositAmount = deposits.Sum(x => x.Amount);

            var totalAmount = subtotal - discountAmount + adjustmentAmount + shippingAmount + taxAmount;
            var amountDue = totalAmount - depositAmount;

            return (subtotal, discountAmount, adjustmentAmount, shippingAmount,
                    taxAmount, totalAmount, depositAmount, amountDue);
        }

        private async Task DeleteAttachments(List<Guid> attachmentIds, CancellationToken cancellationToken)
        {
            foreach (var attachmentId in attachmentIds)
            {
                var attachment = await _attachmentRepository.GetByID(attachmentId);
                if (attachment != null)
                {
                    // Delete physical file
                    if (File.Exists(attachment.FilePath))
                    {
                        File.Delete(attachment.FilePath);
                    }

                    // Delete database record
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
                    UploadedBy = Guid.Empty
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
                NoteText = n
            }).ToList();

            await _noteRepository.AddRangeAsync(noteEntities);
            await _noteRepository.SaveChanges();
        }
    }
}
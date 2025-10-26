using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_PurchaseOrder
{
    public class CreatePurchaseOrderHandler : IRequestHandler<CreatePurchaseOrderCommand, ResponseViewModel<CreatePurchaseOrderDto>>
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
        private readonly ILogger<CreatePurchaseOrderHandler> _logger;
        private readonly IWebHostEnvironment _environment;

        public CreatePurchaseOrderHandler(
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<POLineItem> lineItemRepository,
            IGeneralRepository<PODeposit> depositRepository,
            IGeneralRepository<POShippingCharge> shippingRepository,
            IGeneralRepository<PODiscount> discountRepository,
            IGeneralRepository<POAdjustment> adjustmentRepository,
            IGeneralRepository<POAttachment> attachmentRepository,
            IGeneralRepository<PONote> noteRepository,
            ILogger<CreatePurchaseOrderHandler> logger,
            IMapper mapper,
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

        public async Task<ResponseViewModel<CreatePurchaseOrderDto>> Handle(
            CreatePurchaseOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating purchase order for Company: {CompanyId}, Supplier: {SupplierId}",
                    request.CompanyId, request.SupplierId);

                // Validate Company and Supplier exist
                await ValidateEntities(request, cancellationToken);

                // Generate PO Number
                var poNumber = await GeneratePONumber(request.CompanyId, cancellationToken);

                // Calculate amounts
                var calculations = CalculateAmounts(request);

                // Create PurchaseOrder entity
                var purchaseOrder = new PurchaseOrder
                {
                    Id = Guid.NewGuid(),
                    PONumber = poNumber,
                    CompanyId = request.CompanyId,
                    SupplierId = request.SupplierId,
                    CurrencyCode = request.CurrencyCode,
                    PODate = request.PODate,
                    PaymentTerms = request.PaymentTerms,
                    Notes = request.Notes,
                    Terms = request.Terms,
                    Subtotal = calculations.Subtotal,
                    DiscountAmount = calculations.DiscountAmount,
                    AdjustmentAmount = calculations.AdjustmentAmount,
                    ShippingAmount = calculations.ShippingAmount,
                    TaxAmount = calculations.TaxAmount,
                    TotalAmount = calculations.TotalAmount,
                    DepositAmount = calculations.DepositAmount,
                    AmountDue = calculations.AmountDue,
                    ReceptionStatus = "NotReceived",
                    PaymentStatus = calculations.DepositAmount > 0 ? "PartiallyPaid" : "Unpaid",
                    DocumentStatus = "Draft",
                    CreatedAt = DateTime.UtcNow
                };

                await _poRepository.AddAsync(purchaseOrder);
                await _poRepository.SaveChanges();

                _logger.LogInformation("Purchase Order created with ID: {POId}, Number: {PONumber}",
                    purchaseOrder.Id, purchaseOrder.PONumber);

                // Add Line Items
                await AddLineItems(purchaseOrder.Id, request.LineItems, cancellationToken);

                // Add Deposits
                if (request.Deposits?.Any() == true)
                    await AddDeposits(purchaseOrder.Id, request.Deposits, cancellationToken);

                // Add Shipping Charges
                if (request.ShippingCharges?.Any() == true)
                    await AddShippingCharges(purchaseOrder.Id, request.ShippingCharges, cancellationToken);

                // Add Discounts
                if (request.Discounts?.Any() == true)
                    await AddDiscounts(purchaseOrder.Id, request.Discounts, cancellationToken);

                // Add Adjustments
                if (request.Adjustments?.Any() == true)
                    await AddAdjustments(purchaseOrder.Id, request.Adjustments, cancellationToken);

                // Add Attachments
                if (request.Attachments?.Any() == true)
                    await AddAttachments(purchaseOrder.Id, request.Attachments, cancellationToken);

                // Add Notes
                if (request.PONotes?.Any() == true)
                    await AddNotes(purchaseOrder.Id, request.PONotes, cancellationToken);

                // Map to DTO
                var result = _mapper.Map<CreatePurchaseOrderDto>(purchaseOrder);

                _logger.LogInformation("Purchase Order {PONumber} created successfully", poNumber);

                return ResponseViewModel<CreatePurchaseOrderDto>.Success(
                    result,
                    "Purchase Order created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase order");
                throw;
            }
        }

        private async Task ValidateEntities(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            // Add validation for Company and Supplier existence
            // This would require injecting repositories for Company and Supplier
        }

        private async Task<string> GeneratePONumber(Guid companyId, CancellationToken cancellationToken)
        {
            var lastPO = await _poRepository
                .GetByCompanyId(companyId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.PONumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(lastPO))
                return $"PO-{DateTime.UtcNow:yyyyMM}-0001";

            var parts = lastPO.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
            {
                var newNumber = lastNumber + 1;
                return $"PO-{DateTime.UtcNow:yyyyMM}-{newNumber:D4}";
            }

            return $"PO-{DateTime.UtcNow:yyyyMM}-0001";
        }

        private (decimal Subtotal, decimal DiscountAmount, decimal AdjustmentAmount,
                 decimal ShippingAmount, decimal TaxAmount, decimal TotalAmount,
                 decimal DepositAmount, decimal AmountDue) CalculateAmounts(CreatePurchaseOrderCommand request)
        {
            // Calculate line items subtotal
            decimal subtotal = 0;
            decimal totalTax = 0;

            foreach (var item in request.LineItems)
            {
                var lineAmount = item.Quantity * item.UnitPrice;
                var lineDiscountAmount = lineAmount * (item.DiscountPercent / 100);
                var netAmount = lineAmount - lineDiscountAmount;

                // Tax calculation would require TaxProfile lookup
                // For now, assuming 15% VAT if TaxProfileId is provided
                decimal itemTax = item.TaxProfileId.HasValue ? netAmount * 0.15m : 0;

                subtotal += lineAmount;
                totalTax += itemTax;
            }

            // Calculate discounts
            decimal discountAmount = 0;
            foreach (var discount in request.Discounts ?? new List<CreatePODiscountDto>())
            {
                if (discount.DiscountType == "Percentage")
                    discountAmount += subtotal * (discount.DiscountValue / 100);
                else
                    discountAmount += discount.DiscountValue;
            }

            // Calculate adjustments
            decimal adjustmentAmount = request.Adjustments?.Sum(x => x.Amount) ?? 0;

            // Calculate shipping
            decimal shippingAmount = 0;
            decimal shippingTax = 0;
            foreach (var shipping in request.ShippingCharges ?? new List<CreatePOShippingChargeDto>())
            {
                shippingAmount += shipping.ShippingFee;
                if (shipping.TaxProfileId.HasValue)
                    shippingTax += shipping.ShippingFee * 0.15m; // Assuming 15% VAT
            }

            totalTax += shippingTax;

            // Calculate totals
            decimal totalAmount = subtotal - discountAmount + adjustmentAmount + shippingAmount + totalTax;
            decimal depositAmount = request.Deposits?.Sum(x => x.Amount) ?? 0;
            decimal amountDue = totalAmount - depositAmount;

            return (subtotal, discountAmount, adjustmentAmount, shippingAmount,
                    totalTax, totalAmount, depositAmount, amountDue);
        }

        private async Task AddLineItems(Guid purchaseOrderId, List<CreatePOLineItemDto> items,
            CancellationToken cancellationToken)
        {
            var lineItems = new List<POLineItem>();

            foreach (var item in items)
            {
                var lineAmount = item.Quantity * item.UnitPrice;
                var discountAmount = lineAmount * (item.DiscountPercent / 100);
                var netAmount = lineAmount - discountAmount;
                var taxAmount = item.TaxProfileId.HasValue ? netAmount * 0.15m : 0;
                var lineTotal = netAmount + taxAmount;

                lineItems.Add(new POLineItem
                {
                    Id = Guid.NewGuid(),
                    PurchaseOrderId = purchaseOrderId,
                    ProductId = item.ProductId,
                    ServiceId = item.ServiceId,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    DiscountPercent = item.DiscountPercent,
                    DiscountAmount = discountAmount,
                    TaxProfileId = item.TaxProfileId,
                    TaxAmount = taxAmount,
                    LineTotal = lineTotal,
                    RemainingQuantity = item.Quantity
                });
            }

            await _lineItemRepository.AddRangeAsync(lineItems);
            await _lineItemRepository.SaveChanges();
        }

        private async Task AddDeposits(Guid purchaseOrderId, List<CreatePODepositDto> deposits,
            CancellationToken cancellationToken)
        {
            var depositEntities = deposits.Select(d => new PODeposit
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = purchaseOrderId,
                Amount = d.Amount,
                Percentage = d.Percentage,
                PaymentMethod = d.PaymentMethod,
                ReferenceNumber = d.ReferenceNumber,
                AlreadyPaid = d.AlreadyPaid,
                PaymentDate = d.PaymentDate,
                Notes = d.Notes
            }).ToList();

            await _depositRepository.AddRangeAsync(depositEntities);
            await _depositRepository.SaveChanges();
        }

        private async Task AddShippingCharges(Guid purchaseOrderId,
            List<CreatePOShippingChargeDto> shippingCharges, CancellationToken cancellationToken)
        {
            var shippingEntities = shippingCharges.Select(s =>
            {
                var taxAmount = s.TaxProfileId.HasValue ? s.ShippingFee * 0.15m : 0;
                return new POShippingCharge
                {
                    Id = Guid.NewGuid(),
                    PurchaseOrderId = purchaseOrderId,
                    ShippingFee = s.ShippingFee,
                    TaxProfileId = s.TaxProfileId,
                    TaxAmount = taxAmount,
                    Total = s.ShippingFee + taxAmount,
                    Description = s.Description
                };
            }).ToList();

            await _shippingRepository.AddRangeAsync(shippingEntities);
            await _shippingRepository.SaveChanges();
        }

        private async Task AddDiscounts(Guid purchaseOrderId, List<CreatePODiscountDto> discounts,
            CancellationToken cancellationToken)
        {
            var discountEntities = discounts.Select(d => new PODiscount
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = purchaseOrderId,
                DiscountType = d.DiscountType,
                DiscountValue = d.DiscountValue,
                DiscountAmount = d.DiscountValue, // This should be calculated based on type
                Description = d.Description
            }).ToList();

            await _discountRepository.AddRangeAsync(discountEntities);
            await _discountRepository.SaveChanges();
        }

        private async Task AddAdjustments(Guid purchaseOrderId, List<CreatePOAdjustmentDto> adjustments,
            CancellationToken cancellationToken)
        {
            var adjustmentEntities = adjustments.Select(a => new POAdjustment
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = purchaseOrderId,
                AdjustmentType = a.AdjustmentType,
                Amount = a.Amount,
                Notes = a.Notes
            }).ToList();

            await _adjustmentRepository.AddRangeAsync(adjustmentEntities);
            await _adjustmentRepository.SaveChanges();
        }

        private async Task AddAttachments(Guid purchaseOrderId, List<IFormFile> attachments,
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
                    UploadedBy = Guid.Empty // Should be set from current user context
                });
            }

            await _attachmentRepository.AddRangeAsync(attachmentEntities);
            await _attachmentRepository.SaveChanges();
        }

        private async Task AddNotes(Guid purchaseOrderId, List<string> notes,
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
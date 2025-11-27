using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;
using Serilog;
using ModularERP.Modules.Inventory.Features.Services.Models;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Modules.Purchases.Payment.Models;
using Microsoft.Extensions.Logging;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_PurchaseOrder
{
    public class CreatePurchaseOrderHandler : IRequestHandler<CreatePurchaseOrderCommand, ResponseViewModel<CreatePurchaseOrderDto>>
    {
        private readonly IGeneralRepository<PaymentTerm> _paymentTermRepository;
        private readonly IGeneralRepository<PaymentMethod> _paymentMethodRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IGeneralRepository<POLineItem> _lineItemRepository;
        private readonly IGeneralRepository<PODeposit> _depositRepository;
        private readonly IGeneralRepository<POShippingCharge> _shippingRepository;
        private readonly IGeneralRepository<PODiscount> _discountRepository;
        private readonly IGeneralRepository<POAdjustment> _adjustmentRepository;
        private readonly IGeneralRepository<POAttachment> _attachmentRepository;
        private readonly IGeneralRepository<PONote> _noteRepository;
        private readonly IGeneralRepository<Supplier> _supplierRepository;
        private readonly IGeneralRepository<Company> _companyRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<Service> _serviceRepository;
        private readonly IGeneralRepository<TaxProfile> _taxProfileRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreatePurchaseOrderHandler> _logger;
        private readonly IWebHostEnvironment _environment;

        public CreatePurchaseOrderHandler(
            IGeneralRepository<PaymentTerm> paymentTermRepository,
            IGeneralRepository<PaymentMethod> paymentMethodRepository, 
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<POLineItem> lineItemRepository,
            IGeneralRepository<PODeposit> depositRepository,
            IGeneralRepository<POShippingCharge> shippingRepository,
            IGeneralRepository<PODiscount> discountRepository,
            IGeneralRepository<POAdjustment> adjustmentRepository,
            IGeneralRepository<POAttachment> attachmentRepository,
            IGeneralRepository<PONote> noteRepository,
            IGeneralRepository<Supplier> supplierRepository,
            IGeneralRepository<Company> companyRepository,
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<Service> serviceRepository,
            IGeneralRepository<TaxProfile> taxProfileRepository,
            ILogger<CreatePurchaseOrderHandler> logger,
            IMapper mapper,
            IWebHostEnvironment environment)
        {
            _poRepository = poRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _lineItemRepository = lineItemRepository;
            _depositRepository = depositRepository;
            _shippingRepository = shippingRepository;
            _discountRepository = discountRepository;
            _adjustmentRepository = adjustmentRepository;
            _attachmentRepository = attachmentRepository;
            _noteRepository = noteRepository;
            _supplierRepository = supplierRepository;
            _companyRepository = companyRepository;
            _productRepository = productRepository;
            _serviceRepository = serviceRepository;
            _taxProfileRepository = taxProfileRepository;
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

                // Validate Company and Supplier exist and get supplier info
                var supplier = await ValidateEntitiesAndGetSupplier(request, cancellationToken);

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
                    PaymentTermId = request.PaymentTermId,
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
                    ReceptionStatus = ReceptionStatus.NotReceived,
                    PaymentStatus = calculations.DepositAmount > 0 ? PaymentStatus.PartiallyPaid : PaymentStatus.Unpaid,
                    DocumentStatus = DocumentStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53") // Placeholder for current user
                };

                await _poRepository.AddAsync(purchaseOrder);
                await _poRepository.SaveChanges();

                _logger.LogInformation("Purchase Order created with ID: {POId}, Number: {PONumber}",
                    purchaseOrder.Id, purchaseOrder.PONumber);

                // Add Line Items
                var lineItems = await AddLineItems(purchaseOrder.Id, request.LineItems, cancellationToken);

                // Add Deposits
                List<PODeposit> deposits = new();
                if (request.Deposits?.Any() == true)
                    deposits = await AddDeposits(purchaseOrder.Id, request.Deposits, cancellationToken);

                // Add Shipping Charges
                List<POShippingCharge> shippingCharges = new();
                if (request.ShippingCharges?.Any() == true)
                    shippingCharges = await AddShippingCharges(purchaseOrder.Id, request.ShippingCharges, cancellationToken);

                // Add Discounts
                List<PODiscount> discounts = new();
                if (request.Discounts?.Any() == true)
                    discounts = await AddDiscounts(purchaseOrder.Id, request.Discounts, cancellationToken);

                // Add Adjustments
                List<POAdjustment> adjustments = new();
                if (request.Adjustments?.Any() == true)
                    adjustments = await AddAdjustments(purchaseOrder.Id, request.Adjustments, cancellationToken);

                // Add Attachments
                int attachmentCount = 0;
                if (request.Attachments?.Any() == true)
                    attachmentCount = await AddAttachments(purchaseOrder.Id, request.Attachments, cancellationToken);

                // Add Notes
                if (request.PONotes?.Any() == true)
                    await AddNotes(purchaseOrder.Id, request.PONotes, cancellationToken);

                // Build complete response with all related data
                var result = await BuildResponseDto(purchaseOrder, supplier, lineItems, deposits,
                    shippingCharges, discounts, adjustments, request.PONotes, attachmentCount, cancellationToken);

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

        private async Task<Supplier> ValidateEntitiesAndGetSupplier(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            // Validate Company
            var company = await _companyRepository.GetByIDWithTracking(request.CompanyId);
            if (company == null)
                throw new NotFoundException($"Company with ID {request.CompanyId} not found", FinanceErrorCode.NotFound);

            // Validate Supplier and return it
            var supplier = await _supplierRepository.GetByIDWithTracking(request.SupplierId);
            if (supplier == null)
                throw new NotFoundException($"Supplier with ID {request.SupplierId} not found", FinanceErrorCode.NotFound);

            return supplier;
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
                var currentYearMonth = DateTime.UtcNow.ToString("yyyyMM");
                var lastYearMonth = parts[1];

                // Reset numbering if month changed
                if (currentYearMonth != lastYearMonth)
                    return $"PO-{currentYearMonth}-0001";

                var newNumber = lastNumber + 1;
                return $"PO-{currentYearMonth}-{newNumber:D4}";
            }

            return $"PO-{DateTime.UtcNow:yyyyMM}-0001";
        }

        /// <summary>
        /// Calculates all amounts for Purchase Order according to BRSD requirements
        /// Flow: Line Items → Line Discounts → PO Discounts → Adjustments → Shipping → Taxes → Deposit → Amount Due
        /// </summary>
        private (decimal Subtotal, decimal DiscountAmount, decimal AdjustmentAmount,
                 decimal ShippingAmount, decimal TaxAmount, decimal TotalAmount,
                 decimal DepositAmount, decimal AmountDue) CalculateAmounts(CreatePurchaseOrderCommand request)
        {
            _logger.LogInformation("Starting PO amount calculations");

            // =====================================================
            // STEP 1: Calculate Line Items Totals
            // =====================================================
            decimal lineItemsGrossTotal = 0;      // Sum of (Qty × Price) BEFORE line discounts
            decimal lineItemsNetTotal = 0;        // Sum AFTER line discounts
            decimal lineItemsTaxTotal = 0;        // Tax on net amounts

            foreach (var item in request.LineItems)
            {
                // Gross amount before discount
                var lineGrossAmount = item.Quantity * item.UnitPrice;
                lineItemsGrossTotal += lineGrossAmount;

                // Line discount calculation
                var lineDiscountAmount = lineGrossAmount * (item.DiscountPercent / 100);

                // Net amount after line discount
                var lineNetAmount = lineGrossAmount - lineDiscountAmount;
                lineItemsNetTotal += lineNetAmount;

                // Tax calculation on net amount (after line discount)
                if (item.TaxProfileId.HasValue)
                {
                    var lineTax = lineNetAmount * 0.15m; // 15% VAT
                    lineItemsTaxTotal += lineTax;
                }

                _logger.LogDebug(
                    "Line Item: Gross={Gross}, Discount={Discount}, Net={Net}, Tax={Tax}",
                    lineGrossAmount, lineDiscountAmount, lineNetAmount, item.TaxProfileId.HasValue ? lineNetAmount * 0.15m : 0);
            }

            _logger.LogInformation(
                "Line Items Summary - Gross: {Gross}, Net: {Net}, Tax: {Tax}",
                lineItemsGrossTotal, lineItemsNetTotal, lineItemsTaxTotal);

            // =====================================================
            // STEP 2: Apply PO-Level Discounts (on Net Total)
            // =====================================================
            decimal poDiscountAmount = 0;

            if (request.Discounts?.Any() == true)
            {
                foreach (var discount in request.Discounts)
                {
                    decimal discountValue = 0;

                    if (discount.DiscountType == "Percentage")
                    {
                        // ✅ CRITICAL FIX: Apply percentage on LINE ITEMS NET TOTAL (after line discounts)
                        discountValue = lineItemsNetTotal * (discount.DiscountValue / 100);

                        _logger.LogDebug(
                            "PO Discount ({Type}): {Percent}% of {Base} = {Amount}",
                            discount.DiscountType, discount.DiscountValue, lineItemsNetTotal, discountValue);
                    }
                    else if (discount.DiscountType == "Fixed" || discount.DiscountType == "Amount")
                    {
                        // Fixed amount discount
                        discountValue = discount.DiscountValue;

                        _logger.LogDebug(
                            "PO Discount ({Type}): Fixed Amount = {Amount}",
                            discount.DiscountType, discountValue);
                    }

                    poDiscountAmount += discountValue;
                }
            }

            // Calculate net after PO discount
            decimal netAfterPODiscount = lineItemsNetTotal - poDiscountAmount;

            _logger.LogInformation(
                "After PO Discount - Discount: {Discount}, Net: {Net}",
                poDiscountAmount, netAfterPODiscount);

            // =====================================================
            // STEP 3: Add Adjustments (before final tax calculation)
            // =====================================================
            decimal adjustmentAmount = 0;

            if (request.Adjustments?.Any() == true)
            {
                adjustmentAmount = request.Adjustments.Sum(x => x.Amount);

                _logger.LogInformation("Adjustments Total: {Amount}", adjustmentAmount);
            }

            // =====================================================
            // STEP 4: Calculate Shipping with Tax
            // =====================================================
            decimal shippingFeeTotal = 0;
            decimal shippingTaxTotal = 0;

            if (request.ShippingCharges?.Any() == true)
            {
                foreach (var shipping in request.ShippingCharges)
                {
                    shippingFeeTotal += shipping.ShippingFee;

                    if (shipping.TaxProfileId.HasValue)
                    {
                        var shippingTax = shipping.ShippingFee * 0.15m; // 15% VAT on shipping
                        shippingTaxTotal += shippingTax;

                        _logger.LogDebug(
                            "Shipping Charge: Fee={Fee}, Tax={Tax}",
                            shipping.ShippingFee, shippingTax);
                    }
                }
            }

            _logger.LogInformation(
                "Shipping Summary - Fee: {Fee}, Tax: {Tax}",
                shippingFeeTotal, shippingTaxTotal);

            // =====================================================
            // STEP 5: Calculate Grand Total
            // =====================================================
            // Total tax = line items tax + shipping tax
            decimal totalTax = lineItemsTaxTotal + shippingTaxTotal;

            // Grand Total = Net Items (after all discounts) + Adjustments + Shipping Fee + All Taxes
            decimal grandTotal = netAfterPODiscount + adjustmentAmount + shippingFeeTotal + totalTax;

            _logger.LogInformation(
                "Grand Total Calculation: Net={Net} + Adj={Adj} + Ship={Ship} + Tax={Tax} = {Total}",
                netAfterPODiscount, adjustmentAmount, shippingFeeTotal, totalTax, grandTotal);

            // =====================================================
            // STEP 6: Process and Validate Deposits
            // =====================================================
            decimal depositAmount = 0;

            if (request.Deposits?.Any() == true)
            {
                foreach (var deposit in request.Deposits)
                {
                    decimal calculatedDeposit = 0;

                    // Handle percentage-based deposits
                    if (deposit.Percentage.HasValue && deposit.Percentage.Value > 0)
                    {
                        // ✅ CRITICAL FIX: Calculate deposit based on percentage of grand total
                        calculatedDeposit = grandTotal * (deposit.Percentage.Value / 100);

                        _logger.LogDebug(
                            "Deposit Calculation: {Percent}% of {Total} = {Amount}",
                            deposit.Percentage.Value, grandTotal, calculatedDeposit);

                        // Validate if both amount and percentage are provided
                        if (deposit.Amount > 0)
                        {
                            var difference = Math.Abs(deposit.Amount - calculatedDeposit);

                            // Allow small rounding difference (0.01)
                            if (difference > 0.01m)
                            {
                                _logger.LogWarning(
                                    "⚠️ Deposit Mismatch: Provided Amount={Provided}, Calculated from {Percent}%={Calculated}. Using calculated value.",
                                    deposit.Amount, deposit.Percentage.Value, calculatedDeposit);
                            }
                        }

                        depositAmount += calculatedDeposit;
                    }
                    else if (deposit.Amount > 0)
                    {
                        // Use fixed amount if no percentage provided
                        calculatedDeposit = deposit.Amount;
                        depositAmount += calculatedDeposit;

                        _logger.LogDebug("Deposit: Fixed Amount = {Amount}", calculatedDeposit);
                    }
                }

                // ✅ CRITICAL FIX: Validate deposit doesn't exceed grand total
                if (depositAmount > grandTotal)
                {
                    _logger.LogWarning(
                        "⚠️ Deposit ({Deposit}) exceeds Grand Total ({Total}). Capping deposit to total amount.",
                        depositAmount, grandTotal);

                    depositAmount = grandTotal;
                }

                _logger.LogInformation("Total Deposit Amount: {Amount}", depositAmount);
            }

            // =====================================================
            // STEP 7: Calculate Amount Due
            // =====================================================
            // ✅ CRITICAL FIX: Amount due should never be negative
            decimal amountDue = Math.Max(0, grandTotal - depositAmount);

            _logger.LogInformation(
                "Amount Due: {Total} - {Deposit} = {Due}",
                grandTotal, depositAmount, amountDue);

            // =====================================================
            // STEP 8: Return All Calculated Values
            // =====================================================
            var result = (
                Subtotal: lineItemsGrossTotal,              // ✅ Gross total before line discounts
                DiscountAmount: poDiscountAmount,           // ✅ PO-level discount only (applied on net)
                AdjustmentAmount: adjustmentAmount,         // ✅ Additional adjustments
                ShippingAmount: shippingFeeTotal,           // ✅ Shipping fee (without tax)
                TaxAmount: totalTax,                        // ✅ All taxes combined (items + shipping)
                TotalAmount: grandTotal,                    // ✅ Grand total
                DepositAmount: depositAmount,               // ✅ Validated deposit amount
                AmountDue: amountDue                        // ✅ Remaining balance (non-negative)
            );

            _logger.LogInformation(
                "✅ Final Calculations Complete - Subtotal: {Subtotal}, Discount: {Discount}, " +
                "Adjustment: {Adjustment}, Shipping: {Shipping}, Tax: {Tax}, " +
                "Total: {Total}, Deposit: {Deposit}, Due: {Due}",
                result.Subtotal, result.DiscountAmount, result.AdjustmentAmount,
                result.ShippingAmount, result.TaxAmount, result.TotalAmount,
                result.DepositAmount, result.AmountDue);

            return result;
        }

        private async Task<CreatePurchaseOrderDto> BuildResponseDto(
            PurchaseOrder purchaseOrder,
            Supplier supplier,
            List<POLineItem> lineItems,
            List<PODeposit> deposits,
            List<POShippingCharge> shippingCharges,
            List<PODiscount> discounts,
            List<POAdjustment> adjustments,
            List<string>? poNotes,
            int attachmentCount,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Building response DTO for PO: {PONumber}", purchaseOrder.PONumber);

                // =====================================================
                // STEP 1: Collect All IDs for Batch Fetching
                // =====================================================
                _logger.LogDebug("Step 1: Collecting IDs");

                var productIds = lineItems
                    .Where(x => x.ProductId.HasValue)
                    .Select(x => x.ProductId!.Value)
                    .Distinct()
                    .ToList();

                var serviceIds = lineItems
                    .Where(x => x.ServiceId.HasValue)
                    .Select(x => x.ServiceId!.Value)
                    .Distinct()
                    .ToList();

                var taxProfileIds = lineItems
                    .Where(x => x.TaxProfileId.HasValue)
                    .Select(x => x.TaxProfileId!.Value)
                    .Union(shippingCharges.Where(x => x.TaxProfileId.HasValue).Select(x => x.TaxProfileId!.Value))
                    .Distinct()
                    .ToList();

                // ✅ FIX: Safely collect Payment Method IDs
                var paymentMethodIds = new List<Guid>();
                if (deposits != null && deposits.Any())
                {
                    paymentMethodIds = deposits
                        .Select(x => x.PaymentMethodId)
                        .Distinct()
                        .ToList();

                    _logger.LogInformation("Found {Count} unique payment method IDs: {Ids}",
                        paymentMethodIds.Count, string.Join(", ", paymentMethodIds));
                }

                // =====================================================
                // STEP 2: Fetch All Related Entities in Parallel
                // =====================================================
                _logger.LogDebug("Step 2: Fetching related entities");

                Dictionary<Guid, Product> products;
                Dictionary<Guid, Service> services;
                Dictionary<Guid, TaxProfile> taxProfiles;
                Dictionary<Guid, PaymentMethod> paymentMethods;
                PaymentTerm? paymentTerm = null;

                try
                {
                    var productsTask = productIds.Any()
                        ? _productRepository.GetAll()
                            .Where(p => productIds.Contains(p.Id))
                            .ToDictionaryAsync(p => p.Id, cancellationToken)
                        : Task.FromResult(new Dictionary<Guid, Product>());

                    var servicesTask = serviceIds.Any()
                        ? _serviceRepository.GetAll()
                            .Where(s => serviceIds.Contains(s.Id))
                            .ToDictionaryAsync(s => s.Id, cancellationToken)
                        : Task.FromResult(new Dictionary<Guid, Service>());

                    var taxProfilesTask = taxProfileIds.Any()
                        ? _taxProfileRepository.GetAll()
                            .Where(t => taxProfileIds.Contains(t.Id))
                            .ToDictionaryAsync(t => t.Id, cancellationToken)
                        : Task.FromResult(new Dictionary<Guid, TaxProfile>());

                    // ✅ FIX: Fetch Payment Methods with detailed logging
                    Task<Dictionary<Guid, PaymentMethod>> paymentMethodsTask;
                    if (paymentMethodIds.Any())
                    {
                        _logger.LogInformation("Fetching payment methods for IDs: {Ids}", string.Join(", ", paymentMethodIds));

                        if (_paymentMethodRepository == null)
                        {
                            _logger.LogError("PaymentMethodRepository is NULL!");
                            paymentMethodsTask = Task.FromResult(new Dictionary<Guid, PaymentMethod>());
                        }
                        else
                        {
                            paymentMethodsTask = _paymentMethodRepository.GetAll()
                                .Where(pm => paymentMethodIds.Contains(pm.Id))
                                .ToDictionaryAsync(pm => pm.Id, cancellationToken);
                        }
                    }
                    else
                    {
                        paymentMethodsTask = Task.FromResult(new Dictionary<Guid, PaymentMethod>());
                    }

                    // ✅ Fetch PaymentTerm separately
                    Task<PaymentTerm?> paymentTermTask;
                    if (purchaseOrder.PaymentTermId.HasValue)
                    {
                        _logger.LogDebug("Fetching payment term: {PaymentTermId}", purchaseOrder.PaymentTermId.Value);
                        paymentTermTask = _paymentTermRepository?.GetByID(purchaseOrder.PaymentTermId.Value)
                            ?? Task.FromResult<PaymentTerm?>(null);
                    }
                    else
                    {
                        paymentTermTask = Task.FromResult<PaymentTerm?>(null);
                    }

                    // Wait for all tasks
                    _logger.LogDebug("Waiting for all fetch tasks to complete");
                    await Task.WhenAll(productsTask, servicesTask, taxProfilesTask, paymentMethodsTask, paymentTermTask);

                    products = await productsTask ?? new Dictionary<Guid, Product>();
                    services = await servicesTask ?? new Dictionary<Guid, Service>();
                    taxProfiles = await taxProfilesTask ?? new Dictionary<Guid, TaxProfile>();
                    paymentMethods = await paymentMethodsTask ?? new Dictionary<Guid, PaymentMethod>();
                    paymentTerm = await paymentTermTask;

                    _logger.LogInformation("Fetched {ProductCount} products, {ServiceCount} services, {TaxCount} tax profiles, {PaymentMethodCount} payment methods",
                        products.Count, services.Count, taxProfiles.Count, paymentMethods.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching related entities");
                    // Initialize empty dictionaries to prevent null reference
                    products = new Dictionary<Guid, Product>();
                    services = new Dictionary<Guid, Service>();
                    taxProfiles = new Dictionary<Guid, TaxProfile>();
                    paymentMethods = new Dictionary<Guid, PaymentMethod>();
                }

                // =====================================================
                // STEP 3: Get Payment Term Name
                // =====================================================
                _logger.LogDebug("Step 3: Getting payment term name");

                string? paymentTermName = null;
                if (purchaseOrder.PaymentTermId.HasValue)
                {
                    if (paymentTerm != null)
                    {
                        paymentTermName = paymentTerm.Name;
                        _logger.LogInformation("Payment term found: {PaymentTermName}", paymentTermName);
                    }
                    else
                    {
                        _logger.LogWarning("Payment term with ID {PaymentTermId} not found", purchaseOrder.PaymentTermId.Value);
                    }
                }

                // =====================================================
                // STEP 4: Map Line Items
                // =====================================================
                _logger.LogDebug("Step 4: Mapping {Count} line items", lineItems?.Count ?? 0);

                var lineItemDtos = (lineItems ?? new List<POLineItem>()).Select(li => new POLineItemDto
                {
                    Id = li.Id,
                    ProductId = li.ProductId,
                    ProductName = li.ProductId.HasValue && products.TryGetValue(li.ProductId.Value, out var product)
                        ? product.Name
                        : null,
                    ServiceId = li.ServiceId,
                    ServiceName = li.ServiceId.HasValue && services.TryGetValue(li.ServiceId.Value, out var service)
                        ? service.Name
                        : null,
                    Description = li.Description,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice,
                    DiscountPercent = li.DiscountPercent,
                    DiscountAmount = li.DiscountAmount,
                    TaxProfileId = li.TaxProfileId,
                    TaxProfileName = li.TaxProfileId.HasValue && taxProfiles.TryGetValue(li.TaxProfileId.Value, out var taxProfile)
                        ? taxProfile.Name
                        : null,
                    TaxAmount = li.TaxAmount,
                    LineTotal = li.LineTotal,
                    RemainingQuantity = li.RemainingQuantity
                }).ToList();

                // =====================================================
                // STEP 5: Map Deposits with Payment Method Details
                // =====================================================
                _logger.LogDebug("Step 5: Mapping {Count} deposits", deposits?.Count ?? 0);

                var depositDtos = new List<PODepositDto>();

                if (deposits != null && deposits.Any())
                {
                    foreach (var d in deposits)
                    {
                        try
                        {
                            _logger.LogDebug("Mapping deposit {DepositId} with PaymentMethodId {PaymentMethodId}",
                                d.Id, d.PaymentMethodId);

                            PaymentMethod? paymentMethod = null;

                            // ✅ Safe lookup
                            if (paymentMethods != null && paymentMethods.ContainsKey(d.PaymentMethodId))
                            {
                                paymentMethod = paymentMethods[d.PaymentMethodId];
                                _logger.LogDebug("Found payment method: {Name}", paymentMethod?.Name);
                            }
                            else
                            {
                                _logger.LogWarning("Payment method {PaymentMethodId} not found in dictionary", d.PaymentMethodId);
                            }

                            depositDtos.Add(new PODepositDto
                            {
                                Id = d.Id,
                                Amount = d.Amount,
                                Percentage = d.Percentage,
                                PaymentMethodId = d.PaymentMethodId,
                                PaymentMethodName = paymentMethod?.Name ?? "Unknown",
                                PaymentMethodCode = paymentMethod?.Code ?? string.Empty,
                                PaymentMethodRequiresReference = paymentMethod?.RequiresReference ?? false,
                                ReferenceNumber = d.ReferenceNumber,
                                AlreadyPaid = d.AlreadyPaid,
                                PaymentDate = d.PaymentDate,
                                Notes = d.Notes
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error mapping deposit {DepositId}", d.Id);
                            throw;
                        }
                    }
                }

                _logger.LogInformation("Successfully mapped {Count} deposits", depositDtos.Count);

                // =====================================================
                // STEP 6: Map Shipping Charges
                // =====================================================
                _logger.LogDebug("Step 6: Mapping {Count} shipping charges", shippingCharges?.Count ?? 0);

                var shippingChargeDtos = (shippingCharges ?? new List<POShippingCharge>()).Select(sc => new POShippingChargeDto
                {
                    Id = sc.Id,
                    ShippingFee = sc.ShippingFee,
                    TaxProfileId = sc.TaxProfileId,
                    TaxProfileName = sc.TaxProfileId.HasValue && taxProfiles.TryGetValue(sc.TaxProfileId.Value, out var taxProfile)
                        ? taxProfile.Name
                        : null,
                    TaxAmount = sc.TaxAmount,
                    Total = sc.Total,
                    Description = sc.Description
                }).ToList();

                // =====================================================
                // STEP 7: Build Final Response DTO
                // =====================================================
                _logger.LogDebug("Step 7: Building final response DTO");

                var result = new CreatePurchaseOrderDto
                {
                    Id = purchaseOrder.Id,
                    PONumber = purchaseOrder.PONumber,
                    CompanyId = purchaseOrder.CompanyId,
                    SupplierId = purchaseOrder.SupplierId,
                    SupplierName = supplier?.Name ?? "Unknown Supplier",
                    CurrencyCode = purchaseOrder.CurrencyCode,
                    PODate = purchaseOrder.PODate,
                    PaymentTermId = purchaseOrder.PaymentTermId,
                    PaymentTermName = paymentTermName,
                    Notes = purchaseOrder.Notes,
                    Terms = purchaseOrder.Terms,
                    Subtotal = purchaseOrder.Subtotal,
                    DiscountAmount = purchaseOrder.DiscountAmount,
                    AdjustmentAmount = purchaseOrder.AdjustmentAmount,
                    ShippingAmount = purchaseOrder.ShippingAmount,
                    TaxAmount = purchaseOrder.TaxAmount,
                    TotalAmount = purchaseOrder.TotalAmount,
                    DepositAmount = purchaseOrder.DepositAmount,
                    AmountDue = purchaseOrder.AmountDue,
                    ReceptionStatus = purchaseOrder.ReceptionStatus.ToString(),
                    PaymentStatus = purchaseOrder.PaymentStatus.ToString(),
                    DocumentStatus = purchaseOrder.DocumentStatus.ToString(),
                    CreatedAt = purchaseOrder.CreatedAt,
                    LineItems = lineItemDtos,
                    Deposits = depositDtos,
                    Discounts = _mapper?.Map<List<PODiscountDto>>(discounts) ?? new List<PODiscountDto>(),
                    Adjustments = _mapper?.Map<List<POAdjustmentDto>>(adjustments) ?? new List<POAdjustmentDto>(),
                    ShippingCharges = shippingChargeDtos,
                    PONotes = poNotes ?? new List<string>(),
                    AttachmentCount = attachmentCount
                };

                _logger.LogInformation("Successfully built response DTO for PO: {PONumber}", purchaseOrder.PONumber);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building response DTO for PO: {PONumber}. Exception at: {StackTrace}",
                    purchaseOrder?.PONumber ?? "Unknown", ex.StackTrace);
                throw;
            }
        }
        private async Task<List<POLineItem>> AddLineItems(Guid purchaseOrderId, List<CreatePOLineItemDto> items,
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

            return lineItems;
        }

        private async Task<List<PODeposit>> AddDeposits(Guid purchaseOrderId, List<CreatePODepositDto> deposits,
            CancellationToken cancellationToken)
        {
            var depositEntities = deposits.Select(d => new PODeposit
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = purchaseOrderId,
                Amount = d.Amount,
                Percentage = d.Percentage,
                PaymentMethodId = d.PaymentMethodId, // ✅ Changed from PaymentMethod (enum) to PaymentMethodId (Guid)
                ReferenceNumber = d.ReferenceNumber,
                AlreadyPaid = d.AlreadyPaid,
                PaymentDate = d.PaymentDate,
                Notes = d.Notes
            }).ToList();

            await _depositRepository.AddRangeAsync(depositEntities);
            await _depositRepository.SaveChanges();

            return depositEntities;
        }

        private async Task<List<POShippingCharge>> AddShippingCharges(Guid purchaseOrderId,
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

            return shippingEntities;
        }

        private async Task<List<PODiscount>> AddDiscounts(Guid purchaseOrderId, List<CreatePODiscountDto> discounts,
            CancellationToken cancellationToken)
        {
            var discountEntities = discounts.Select(d => new PODiscount
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = purchaseOrderId,
                DiscountType = Enum.Parse<DiscountType>(d.DiscountType),
                DiscountValue = d.DiscountValue,
                DiscountAmount = d.DiscountValue,
                Description = d.Description
            }).ToList();

            await _discountRepository.AddRangeAsync(discountEntities);
            await _discountRepository.SaveChanges();

            return discountEntities;
        }

        private async Task<List<POAdjustment>> AddAdjustments(Guid purchaseOrderId, List<CreatePOAdjustmentDto> adjustments,
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

            return adjustmentEntities;
        }

        private async Task<int> AddAttachments(Guid purchaseOrderId, List<IFormFile> attachments,
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

            return attachmentEntities.Count;
        }

        private async Task AddNotes(Guid purchaseOrderId, List<string> notes,
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
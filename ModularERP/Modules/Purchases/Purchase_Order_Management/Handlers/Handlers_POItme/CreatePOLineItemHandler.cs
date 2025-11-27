using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_POItme;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POItme;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Shared.Interfaces;
using Serilog;
using ModularERP.Common.Enum.Purchases_Enum;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_POItme
{
    public class CreatePOLineItemHandler : IRequestHandler<CreatePOLineItemCommand, ResponseViewModel<POLineItemResponseDto>>
    {
        private readonly IGeneralRepository<POLineItem> _lineItemRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IGeneralRepository<TaxProfile> _taxProfileRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreatePOLineItemHandler> _logger;

        public CreatePOLineItemHandler(
            IGeneralRepository<POLineItem> lineItemRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<TaxProfile> taxProfileRepository,
            IMapper mapper,
            ILogger<CreatePOLineItemHandler> logger)
        {
            _lineItemRepository = lineItemRepository;
            _poRepository = poRepository;
            _taxProfileRepository = taxProfileRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<POLineItemResponseDto>> Handle(
            CreatePOLineItemCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating line item for Purchase Order {PurchaseOrderId}", request.PurchaseOrderId);

                // Verify PO exists and is in valid state
                var po = await _poRepository.GetByID(request.PurchaseOrderId);
                if (po == null)
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Check if PO is in Draft or Submitted status (can add items)
                if (po.DocumentStatus != DocumentStatus.Draft && po.DocumentStatus != DocumentStatus.Submitted)
                {
                    throw new BusinessLogicException(
                        $"Cannot add items to Purchase Order in {po.DocumentStatus} status",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Map DTO to entity
                var lineItem = _mapper.Map<POLineItem>(request.LineItem);
                lineItem.PurchaseOrderId = request.PurchaseOrderId;
                lineItem.CreatedAt = DateTime.UtcNow;
                lineItem.IsActive = true;
                lineItem.IsDeleted = false;

                // ===== CALCULATE LINE TOTALS =====
                var calculationResult = await CalculateLineItemTotals(lineItem, cancellationToken);

                lineItem.DiscountAmount = calculationResult.DiscountAmount;
                lineItem.TaxAmount = calculationResult.TaxAmount;
                lineItem.LineTotal = calculationResult.LineTotal;

                // Calculate remaining quantity
                lineItem.RemainingQuantity = lineItem.Quantity;

                // Add to repository
                await _lineItemRepository.AddAsync(lineItem);
                await _lineItemRepository.SaveChanges();

                _logger.LogInformation(
                    "Line item {LineItemId} created successfully for PO {PurchaseOrderId}. " +
                    "Subtotal: {Subtotal}, Discount: {Discount}, Tax: {Tax}, Total: {Total}",
                    lineItem.Id, request.PurchaseOrderId,
                    calculationResult.Subtotal, calculationResult.DiscountAmount,
                    calculationResult.TaxAmount, calculationResult.LineTotal);

                // Retrieve with projections
                var response = await _lineItemRepository
                    .Get(x => x.Id == lineItem.Id)
                    .ProjectTo<POLineItemResponseDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                return ResponseViewModel<POLineItemResponseDto>.Success(
                    response!,
                    "Line item created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating line item for Purchase Order {PurchaseOrderId}",
                    request.PurchaseOrderId);
                throw;
            }
        }

        private async Task<LineItemCalculation> CalculateLineItemTotals(
            POLineItem lineItem,
            CancellationToken cancellationToken)
        {
            // Step 1: Calculate subtotal before discount
            decimal subtotal = lineItem.Quantity * lineItem.UnitPrice;

            // Step 2: Calculate discount amount
            decimal discountAmount = 0;
            if (lineItem.DiscountPercent > 0)
            {
                // Priority to percentage
                discountAmount = subtotal * (lineItem.DiscountPercent / 100);
            }
            else if (lineItem.DiscountAmount > 0)
            {
                // Use fixed discount if provided and no percentage
                discountAmount = lineItem.DiscountAmount;
            }

            // Step 3: Calculate amount after discount
            decimal amountAfterDiscount = subtotal - discountAmount;

            // Step 4: Calculate tax amount
            decimal taxAmount = 0;
            if (lineItem.TaxProfileId.HasValue)
            {
                taxAmount = await CalculateTaxAmount(
                    lineItem.TaxProfileId.Value,
                    amountAfterDiscount,
                    cancellationToken);
            }

            // Step 5: Calculate final line total
            decimal lineTotal = amountAfterDiscount + taxAmount;

            return new LineItemCalculation
            {
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                AmountAfterDiscount = amountAfterDiscount,
                TaxAmount = taxAmount,
                LineTotal = lineTotal
            };
        }

        private async Task<decimal> CalculateTaxAmount(
            Guid taxProfileId,
            decimal baseAmount,
            CancellationToken cancellationToken)
        {
            var taxProfile = await _taxProfileRepository
                .Get(tp => tp.Id == taxProfileId)
                .Include(tp => tp.TaxProfileComponents)
                    .ThenInclude(tpc => tpc.TaxComponent)
                .FirstOrDefaultAsync(cancellationToken);

            if (taxProfile == null || !taxProfile.TaxProfileComponents.Any())
            {
                _logger.LogWarning("Tax Profile {TaxProfileId} not found or has no components", taxProfileId);
                return 0;
            }

            decimal totalTax = 0;
            decimal cumulativeBase = baseAmount;

            // Sort by priority and calculate taxes
            var components = taxProfile.TaxProfileComponents
                .Where(tpc => !tpc.IsDeleted)
                .OrderBy(tpc => tpc.Priority)
                .ToList();

            foreach (var component in components)
            {
                var taxComponent = component.TaxComponent;
                decimal componentTax = 0;

                switch (taxComponent.RateType)
                {
                    case Common.Enum.Inventory_Enum.TaxRateType.Percentage:
                        componentTax = cumulativeBase * (taxComponent.RateValue / 100);
                        break;

                    case Common.Enum.Inventory_Enum.TaxRateType.Fixed:
                        componentTax = taxComponent.RateValue;
                        break;
                }

                totalTax += componentTax;

                // If tax is inclusive, it should be deducted from base
                // If exclusive, next tax compounds on previous
                if (taxComponent.IncludedType == Common.Enum.Inventory_Enum.TaxIncludedType.Exclusive)
                {
                    cumulativeBase += componentTax; // Compound for next tax
                }

                _logger.LogDebug(
                    "Tax Component: {Name}, Rate: {Rate}, Type: {Type}, Calculated: {Amount}",
                    taxComponent.Name, taxComponent.RateValue, taxComponent.RateType, componentTax);
            }

            return Math.Round(totalTax, 4);
        }

        private class LineItemCalculation
        {
            public decimal Subtotal { get; set; }
            public decimal DiscountAmount { get; set; }
            public decimal AmountAfterDiscount { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal LineTotal { get; set; }
        }
    }
}
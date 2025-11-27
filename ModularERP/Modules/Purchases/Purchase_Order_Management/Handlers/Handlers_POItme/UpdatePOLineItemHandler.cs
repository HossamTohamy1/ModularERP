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
    public class UpdatePOLineItemHandler : IRequestHandler<UpdatePOLineItemCommand, ResponseViewModel<POLineItemResponseDto>>
    {
        private readonly IGeneralRepository<POLineItem> _lineItemRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IGeneralRepository<TaxProfile> _taxProfileRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdatePOLineItemHandler> _logger;

        public UpdatePOLineItemHandler(
            IGeneralRepository<POLineItem> lineItemRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<TaxProfile> taxProfileRepository,
            IMapper mapper,
            ILogger<UpdatePOLineItemHandler> logger)
        {
            _lineItemRepository = lineItemRepository;
            _poRepository = poRepository;
            _taxProfileRepository = taxProfileRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<POLineItemResponseDto>> Handle(
            UpdatePOLineItemCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating line item {LineItemId} for Purchase Order {PurchaseOrderId}",
                    request.LineItemId, request.PurchaseOrderId);

                // Verify PO exists and is in valid state
                var po = await _poRepository.GetByID(request.PurchaseOrderId);
                if (po == null)
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Check if PO is in Draft or Submitted status (can edit items)
                if (po.DocumentStatus != DocumentStatus.Draft && po.DocumentStatus != DocumentStatus.Submitted)
                {
                    throw new BusinessLogicException(
                        $"Cannot edit items in Purchase Order with {po.DocumentStatus} status",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Get existing line item
                var existingLineItem = await _lineItemRepository.GetByIDWithTracking(request.LineItemId);
                if (existingLineItem == null)
                {
                    throw new NotFoundException(
                        $"Line item with ID {request.LineItemId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Verify line item belongs to the specified PO
                if (existingLineItem.PurchaseOrderId != request.PurchaseOrderId)
                {
                    throw new BusinessLogicException(
                        "Line item does not belong to the specified Purchase Order",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Check if item has been received (prevent editing)
                if (existingLineItem.ReceivedQuantity > 0)
                {
                    throw new BusinessLogicException(
                        "Cannot edit line item that has already been partially or fully received",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Map updates
                _mapper.Map(request.LineItem, existingLineItem);
                existingLineItem.UpdatedAt = DateTime.UtcNow;

                // ===== RECALCULATE LINE TOTALS =====
                var calculationResult = await CalculateLineItemTotals(existingLineItem, cancellationToken);

                existingLineItem.DiscountAmount = calculationResult.DiscountAmount;
                existingLineItem.TaxAmount = calculationResult.TaxAmount;
                existingLineItem.LineTotal = calculationResult.LineTotal;

                // Recalculate remaining quantity
                existingLineItem.RemainingQuantity = existingLineItem.Quantity -
                    existingLineItem.ReceivedQuantity - existingLineItem.ReturnedQuantity;

                await _lineItemRepository.SaveChanges();

                _logger.LogInformation(
                    "Line item {LineItemId} updated successfully. " +
                    "Subtotal: {Subtotal}, Discount: {Discount}, Tax: {Tax}, Total: {Total}",
                    request.LineItemId,
                    calculationResult.Subtotal, calculationResult.DiscountAmount,
                    calculationResult.TaxAmount, calculationResult.LineTotal);

                // Retrieve with projections
                var response = await _lineItemRepository
                    .Get(x => x.Id == request.LineItemId)
                    .ProjectTo<POLineItemResponseDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                return ResponseViewModel<POLineItemResponseDto>.Success(
                    response!,
                    "Line item updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating line item {LineItemId}", request.LineItemId);
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
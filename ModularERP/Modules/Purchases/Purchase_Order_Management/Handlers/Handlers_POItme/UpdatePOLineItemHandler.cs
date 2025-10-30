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
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_POItme
{
    public class UpdatePOLineItemHandler : IRequestHandler<UpdatePOLineItemCommand, ResponseViewModel<POLineItemResponseDto>>
    {
        private readonly IGeneralRepository<POLineItem> _lineItemRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdatePOLineItemHandler> _logger;

        public UpdatePOLineItemHandler(
            IGeneralRepository<POLineItem> lineItemRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IMapper mapper,
            ILogger<UpdatePOLineItemHandler> logger)
        {
            _lineItemRepository = lineItemRepository;
            _poRepository = poRepository;
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
                if (po.DocumentStatus != "Draft" && po.DocumentStatus != "Submitted")
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

                // Recalculate remaining quantity
                existingLineItem.RemainingQuantity = existingLineItem.Quantity -
                    existingLineItem.ReceivedQuantity - existingLineItem.ReturnedQuantity;

                await _lineItemRepository.SaveChanges();

                _logger.LogInformation("Line item {LineItemId} updated successfully", request.LineItemId);

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
    }
}

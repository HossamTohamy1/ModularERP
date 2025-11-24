using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundItem;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_RefundItem
{
    [Obsolete("Use UpdateRefundHandler to update entire refund with all line items")]
    public class UpdateRefundItemHandler : IRequestHandler<UpdateRefundItemCommand, ResponseViewModel<RefundItemDto>>
    {
        private readonly IGeneralRepository<RefundLineItem> _repository;
        private readonly IGeneralRepository<PurchaseRefund> _refundRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateRefundItemHandler> _logger;

        public UpdateRefundItemHandler(
            IGeneralRepository<RefundLineItem> repository,
            IGeneralRepository<PurchaseRefund> refundRepository,
            IGeneralRepository<GRNLineItem> grnLineRepo,
            IMapper mapper,
            ILogger<UpdateRefundItemHandler> logger)
        {
            _repository = repository;
            _refundRepository = refundRepository;
            _grnLineRepo = grnLineRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefundItemDto>> Handle(UpdateRefundItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating refund item: {ItemId}", request.ItemId);

                var refundItem = await _repository.GetAll()
                    .Include(ri => ri.GRNLineItem)
                    .Include(ri => ri.Refund)
                        .ThenInclude(r => r.LineItems)
                    .FirstOrDefaultAsync(ri => ri.Id == request.ItemId, cancellationToken);

                if (refundItem == null)
                {
                    throw new NotFoundException("Refund item not found", FinanceErrorCode.NotFound);
                }

                if (refundItem.RefundId != request.RefundId)
                {
                    throw new BusinessLogicException(
                        "Item does not belong to this refund",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Validate new quantity
                var otherReturned = refundItem.Refund.LineItems
                    .Where(rl => rl.GRNLineItemId == refundItem.GRNLineItemId && rl.Id != refundItem.Id)
                    .Sum(rl => rl.ReturnQuantity);

                var availableToReturn = refundItem.GRNLineItem.ReceivedQuantity - otherReturned;

                if (request.ReturnQuantity > availableToReturn)
                {
                    throw new BusinessLogicException(
                        $"Return quantity ({request.ReturnQuantity}) exceeds available quantity ({availableToReturn})",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                refundItem.ReturnQuantity = request.ReturnQuantity;
                refundItem.UnitPrice = request.UnitPrice;
                refundItem.LineTotal = request.ReturnQuantity * request.UnitPrice;
                refundItem.UpdatedAt = DateTime.UtcNow;

                await _repository.Update(refundItem);
                await _repository.SaveChanges();

                // Update Refund Total Amount
                await UpdateRefundTotal(request.RefundId);

                var dto = _mapper.Map<RefundItemDto>(refundItem);

                _logger.LogInformation("Refund item updated successfully. Item ID: {ItemId}", refundItem.Id);
                _logger.LogWarning("DEPRECATED: Consider using UpdateRefundHandler instead");

                return ResponseViewModel<RefundItemDto>.Success(dto, "Refund item updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund item: {ItemId}", request.ItemId);
                throw;
            }
        }

        private async Task UpdateRefundTotal(Guid refundId)
        {
            var total = _repository
                .Get(r => r.RefundId == refundId)
                .Sum(r => r.LineTotal);

            var refund = await _refundRepository.GetByIDWithTracking(refundId);
            if (refund != null)
            {
                refund.TotalAmount = total;
                refund.UpdatedAt = DateTime.UtcNow;
                await _refundRepository.SaveChanges();
            }
        }
    }
}
 
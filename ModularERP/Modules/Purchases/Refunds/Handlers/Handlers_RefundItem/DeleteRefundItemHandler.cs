using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_RefundItem
{
    public class DeleteRefundItemHandler : IRequestHandler<DeleteRefundItemCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<RefundLineItem> _repository;
        private readonly IGeneralRepository<PurchaseRefund> _refundRepository;
        private readonly ILogger<DeleteRefundItemHandler> _logger;

        public DeleteRefundItemHandler(
            IGeneralRepository<RefundLineItem> repository,
            IGeneralRepository<PurchaseRefund> refundRepository,
            ILogger<DeleteRefundItemHandler> logger)
        {
            _repository = repository;
            _refundRepository = refundRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteRefundItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting refund item: {ItemId}", request.ItemId);

                var refundItem = await _repository.GetByIDWithTracking(request.ItemId);
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

                var refundId = refundItem.RefundId;

                await _repository.Delete(request.ItemId);

                // Update Refund Total Amount
                await UpdateRefundTotal(refundId);

                _logger.LogInformation("Refund item deleted successfully. Item ID: {ItemId}", request.ItemId);

                return ResponseViewModel<bool>.Success(true, "Refund item deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting refund item: {ItemId}", request.ItemId);
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
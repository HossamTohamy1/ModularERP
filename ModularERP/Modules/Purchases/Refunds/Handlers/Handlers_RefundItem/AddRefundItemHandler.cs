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
    public class AddRefundItemHandler : IRequestHandler<AddRefundItemCommand, ResponseViewModel<RefundItemDto>>
    {
        private readonly IGeneralRepository<RefundLineItem> _repository;
        private readonly IGeneralRepository<PurchaseRefund> _refundRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<AddRefundItemHandler> _logger;

        public AddRefundItemHandler(
            IGeneralRepository<RefundLineItem> repository,
            IGeneralRepository<PurchaseRefund> refundRepository,
            IGeneralRepository<GRNLineItem> grnLineRepo,
            IMapper mapper,
            ILogger<AddRefundItemHandler> logger)
        {
            _repository = repository;
            _refundRepository = refundRepository;
            _grnLineRepo = grnLineRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefundItemDto>> Handle(AddRefundItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Adding refund item for Refund: {RefundId}, GRN Line: {GRNLineId}",
                    request.RefundId, request.GRNLineItemId);

                // Validate refund exists
                var refund = await _refundRepository.GetAll()
                    .Include(r => r.LineItems)
                    .FirstOrDefaultAsync(r => r.Id == request.RefundId, cancellationToken);

                if (refund == null)
                {
                    throw new NotFoundException("Refund not found", FinanceErrorCode.NotFound);
                }

                // Validate GRN line exists
                var grnLine = await _grnLineRepo.GetAll()
                    .Include(g => g.POLineItem)
                    .FirstOrDefaultAsync(g => g.Id == request.GRNLineItemId, cancellationToken);

                if (grnLine == null)
                {
                    throw new NotFoundException("GRN Line Item not found", FinanceErrorCode.NotFound);
                }

                // Validate quantity
                var alreadyReturned = refund.LineItems
                    .Where(rl => rl.GRNLineItemId == request.GRNLineItemId)
                    .Sum(rl => rl.ReturnQuantity);

                var availableToReturn = grnLine.ReceivedQuantity - alreadyReturned;

                if (request.ReturnQuantity > availableToReturn)
                {
                    throw new BusinessLogicException(
                        $"Return quantity ({request.ReturnQuantity}) exceeds available quantity ({availableToReturn})",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Create line item
                var lineTotal = request.ReturnQuantity * request.UnitPrice;

                var refundItem = new RefundLineItem
                {
                    Id = Guid.NewGuid(),
                    RefundId = request.RefundId,
                    GRNLineItemId = request.GRNLineItemId,
                    ReturnQuantity = request.ReturnQuantity,
                    UnitPrice = request.UnitPrice,
                    LineTotal = lineTotal,
                    CreatedAt = DateTime.UtcNow
                };

                await _repository.AddAsync(refundItem);
                await _repository.SaveChanges();

                // Update Refund Total Amount
                await UpdateRefundTotal(request.RefundId);

                var dto = _mapper.Map<RefundItemDto>(refundItem);

                _logger.LogInformation("Refund item added successfully. Item ID: {ItemId}", refundItem.Id);
                _logger.LogWarning("DEPRECATED: Consider using CreateRefundHandler with complete line items instead");

                return ResponseViewModel<RefundItemDto>.Success(dto, "Refund item added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding refund item for Refund: {RefundId}", request.RefundId);
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

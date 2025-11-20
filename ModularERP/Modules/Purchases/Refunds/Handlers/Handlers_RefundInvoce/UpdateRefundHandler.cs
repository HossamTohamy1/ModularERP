using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_RefundInvoce
{
    public class UpdateRefundHandler : IRequestHandler<UpdateRefundCommand, ResponseViewModel<RefundDto>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IGeneralRepository<RefundLineItem> _lineItemRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateRefundHandler> _logger;

        public UpdateRefundHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IGeneralRepository<RefundLineItem> lineItemRepo,
            IMapper mapper,
            ILogger<UpdateRefundHandler> logger)
        {
            _refundRepo = refundRepo;
            _lineItemRepo = lineItemRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefundDto>> Handle(UpdateRefundCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating refund: {RefundId}", request.Id);

                var refund = await _refundRepo.GetByIDWithTracking(request.Id);
                if (refund == null)
                {
                    throw new NotFoundException(
                        $"Refund with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                // Update basic fields
                refund.RefundDate = request.RefundDate;
                refund.Reason = request.Reason;
                refund.Notes = request.Notes;
                refund.UpdatedAt = DateTime.UtcNow;

                // Delete existing line items
                var existingLines = _lineItemRepo.GetAll().Where(li => li.RefundId == request.Id);
                foreach (var line in existingLines)
                {
                    await _lineItemRepo.Delete(line.Id);
                }

                // Add new line items
                var totalAmount = 0m;
                foreach (var lineItem in request.LineItems)
                {
                    var refundLine = new RefundLineItem
                    {
                        Id = Guid.NewGuid(),
                        RefundId = refund.Id,
                        GRNLineItemId = lineItem.GRNLineItemId,
                        ReturnQuantity = lineItem.ReturnQuantity,
                        UnitPrice = lineItem.UnitPrice,
                        LineTotal = lineItem.ReturnQuantity * lineItem.UnitPrice,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _lineItemRepo.AddAsync(refundLine);
                    totalAmount += refundLine.LineTotal;
                }

                refund.TotalAmount = totalAmount;
                await _refundRepo.SaveChanges();

                _logger.LogInformation("Successfully updated refund: {RefundId}", request.Id);

                // Project to DTO
                var refundDto = await _refundRepo.GetAll()
                    .Where(r => r.Id == request.Id)
                    .Select(r => _mapper.Map<RefundDto>(r))
                    .FirstOrDefaultAsync(cancellationToken);

                return ResponseViewModel<RefundDto>.Success(refundDto!, "Refund updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund: {RefundId}", request.Id);
                throw;
            }
        }
    }
}
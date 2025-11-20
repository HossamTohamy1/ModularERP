using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundInvoce;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_RefundInvoce
{
    public class GetRefundsByPOHandler : IRequestHandler<GetRefundsByPOQuery, ResponseViewModel<List<RefundDto>>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetRefundsByPOHandler> _logger;

        public GetRefundsByPOHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IMapper mapper,
            ILogger<GetRefundsByPOHandler> logger)
        {
            _refundRepo = refundRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<RefundDto>>> Handle(GetRefundsByPOQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving refunds for PO: {PurchaseOrderId}", request.PurchaseOrderId);

                var refunds = await _refundRepo.GetAll()
                    .Where(r => r.PurchaseOrderId == request.PurchaseOrderId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => _mapper.Map<RefundDto>(r))
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} refunds for PO: {PurchaseOrderId}",
                    refunds.Count, request.PurchaseOrderId);

                return ResponseViewModel<List<RefundDto>>.Success(refunds, "Refunds retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refunds for PO: {PurchaseOrderId}", request.PurchaseOrderId);
                throw;
            }
        }
    }
}
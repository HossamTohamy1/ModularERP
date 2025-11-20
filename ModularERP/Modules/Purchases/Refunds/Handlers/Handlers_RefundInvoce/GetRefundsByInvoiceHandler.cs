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
    public class GetRefundsByInvoiceHandler : IRequestHandler<GetRefundsByInvoiceQuery, ResponseViewModel<List<RefundDto>>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetRefundsByInvoiceHandler> _logger;

        public GetRefundsByInvoiceHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IMapper mapper,
            ILogger<GetRefundsByInvoiceHandler> logger)
        {
            _refundRepo = refundRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<RefundDto>>> Handle(GetRefundsByInvoiceQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving refunds for Invoice: {InvoiceId}", request.InvoiceId);

                // Fixed: Navigate through PurchaseOrder.Invoices instead of PurchaseOrder.InvoiceId
                var refunds = await _refundRepo.GetAll()
                    .Where(r => r.PurchaseOrder.Invoices.Any(i => i.Id == request.InvoiceId))
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync(cancellationToken);

                var refundDtos = _mapper.Map<List<RefundDto>>(refunds);

                _logger.LogInformation("Retrieved {Count} refunds for Invoice: {InvoiceId}",
                    refundDtos.Count, request.InvoiceId);

                return ResponseViewModel<List<RefundDto>>.Success(refundDtos, "Refunds retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refunds for Invoice: {InvoiceId}", request.InvoiceId);
                throw;
            }
        }
    }
}
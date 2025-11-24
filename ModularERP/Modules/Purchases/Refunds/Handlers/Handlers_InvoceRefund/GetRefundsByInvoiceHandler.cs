using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundInvoce;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_InvoceRefund
{
    #region Get Refunds by Invoice Handler
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

        public async Task<ResponseViewModel<List<RefundDto>>> Handle(
            GetRefundsByInvoiceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving refunds for Invoice: {InvoiceId}", request.InvoiceId);

                // ✅ Query refunds directly linked to the invoice
                var refunds = await _refundRepo.GetAll()
                    .Include(r => r.PurchaseOrder)
                    .Include(r => r.PurchaseInvoice)
                    .Include(r => r.Supplier)
                    .Include(r => r.DebitNote)
                    .Include(r => r.LineItems)
                        .ThenInclude(li => li.GRNLineItem)
                            .ThenInclude(g => g.POLineItem)
                                .ThenInclude(p => p.Product)
                    .Where(r => r.PurchaseInvoiceId == request.InvoiceId) // ⭐ Direct link
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync(cancellationToken);

                if (!refunds.Any())
                {
                    _logger.LogInformation("No refunds found for Invoice: {InvoiceId}", request.InvoiceId);
                    return ResponseViewModel<List<RefundDto>>.Success(
                        new List<RefundDto>(),
                        "No refunds found for this invoice");
                }

                var refundDtos = _mapper.Map<List<RefundDto>>(refunds);

                _logger.LogInformation("Retrieved {Count} refund(s) for Invoice: {InvoiceId}",
                    refundDtos.Count, request.InvoiceId);

                return ResponseViewModel<List<RefundDto>>.Success(
                    refundDtos,
                    $"Retrieved {refundDtos.Count} refund(s) successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refunds for Invoice: {InvoiceId}", request.InvoiceId);
                throw;
            }
        }
    }
    #endregion
}
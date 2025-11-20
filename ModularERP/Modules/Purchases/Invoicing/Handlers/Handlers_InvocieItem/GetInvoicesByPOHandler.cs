using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_InvocieItem;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_InvocieItem
{
    public class GetInvoicesByPOHandler : IRequestHandler<GetInvoicesByPOQuery, ResponseViewModel<List<InvoiceResponse>>>
    {
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetInvoicesByPOHandler> _logger;

        public GetInvoicesByPOHandler(
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IMapper mapper,
            ILogger<GetInvoicesByPOHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<InvoiceResponse>>> Handle(GetInvoicesByPOQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching invoices for Purchase Order ID: {PurchaseOrderId}", request.PurchaseOrderId);

                var invoices = await _invoiceRepository
                    .GetAll()
                    .Where(x => x.PurchaseOrderId == request.PurchaseOrderId)
                    .ProjectTo<InvoiceResponse>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Successfully fetched {Count} invoices for Purchase Order ID: {PurchaseOrderId}",
                    invoices.Count, request.PurchaseOrderId);

                return ResponseViewModel<List<InvoiceResponse>>.Success(
                    invoices,
                    $"Successfully retrieved {invoices.Count} invoice(s)"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching invoices for Purchase Order ID: {PurchaseOrderId}",
                    request.PurchaseOrderId);

                return ResponseViewModel<List<InvoiceResponse>>.Error(
                    "An error occurred while retrieving invoices",
                    FinanceErrorCode.InternalServerError
                );
            }
        }
    }
}

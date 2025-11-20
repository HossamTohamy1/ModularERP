using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;
using ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_Invoice;
using ModularERP.Modules.Purchases.Invoicing.Services;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_Invoice
{
    public class GetPurchaseInvoiceByIdHandler : IRequestHandler<GetPurchaseInvoiceByIdQuery, ResponseViewModel<PurchaseInvoiceDto>>
    {
        private readonly IPurchaseInvoiceService _invoiceService;
        private readonly ILogger<GetPurchaseInvoiceByIdHandler> _logger;

        public GetPurchaseInvoiceByIdHandler(
            IPurchaseInvoiceService invoiceService,
            ILogger<GetPurchaseInvoiceByIdHandler> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PurchaseInvoiceDto>> Handle(
            GetPurchaseInvoiceByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Retrieving purchase invoice: {InvoiceId}",
                    request.Id);

                var invoice = await _invoiceService.GetInvoiceByIdAsync(
                    request.Id,
                    cancellationToken);

                if (invoice == null)
                {
                    _logger.LogWarning("Purchase invoice not found: {InvoiceId}", request.Id);
                    throw new NotFoundException(
                        $"Purchase invoice with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation(
                    "Purchase invoice retrieved successfully: {InvoiceId}",
                    request.Id);

                return ResponseViewModel<PurchaseInvoiceDto>.Success(
                    invoice,
                    "Purchase invoice retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving purchase invoice: {InvoiceId}",
                    request.Id);
                throw;
            }
        }
    }
}
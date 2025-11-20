using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_InvociePayment;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_InvoicePayment
{
    public class GetInvoicePaymentsHandler : IRequestHandler<GetInvoicePaymentsQuery, ResponseViewModel<GetInvoicePaymentsResponse>>
    {
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IGeneralRepository<SupplierPayment> _paymentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetInvoicePaymentsHandler> _logger;

        public GetInvoicePaymentsHandler(
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IGeneralRepository<SupplierPayment> paymentRepository,
            IMapper mapper,
            ILogger<GetInvoicePaymentsHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<GetInvoicePaymentsResponse>> Handle(
            GetInvoicePaymentsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching payments for invoice {InvoiceId}", request.InvoiceId);

                var invoice = await _invoiceRepository
                    .Get(i => i.Id == request.InvoiceId)
                    .Select(i => new
                    {
                        i.Id,
                        i.InvoiceNumber,
                        i.TotalAmount,
                        i.AmountDue
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (invoice == null)
                {
                    _logger.LogWarning("Invoice not found with ID {InvoiceId}", request.InvoiceId);
                    throw new NotFoundException(
                        $"Invoice with ID {request.InvoiceId} not found",
                        FinanceErrorCode.NotFound);
                }

                var payments = await _paymentRepository
                    .Get(p => p.InvoiceId == request.InvoiceId)
                    .Select(p => new PaymentDto
                    {
                        Id = p.Id,
                        PaymentMethod = p.PaymentMethod,
                        PaymentDate = p.PaymentDate,
                        Amount = p.Amount,
                        ReferenceNumber = p.ReferenceNumber,
                        Notes = p.Notes,
                        CreatedAt = p.CreatedAt
                    })
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync(cancellationToken);

                var totalPaid = payments.Sum(p => p.Amount);

                var response = new GetInvoicePaymentsResponse
                {
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    TotalAmount = invoice.TotalAmount,
                    TotalPaid = totalPaid,
                    AmountDue = invoice.AmountDue,
                    Payments = payments
                };

                _logger.LogInformation(
                    "Successfully retrieved {PaymentCount} payments for invoice {InvoiceId}",
                    payments.Count,
                    request.InvoiceId);

                return ResponseViewModel<GetInvoicePaymentsResponse>.Success(
                    response,
                    "Payments retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for invoice {InvoiceId}", request.InvoiceId);
                throw new BusinessLogicException(
                    "An error occurred while retrieving invoice payments",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}
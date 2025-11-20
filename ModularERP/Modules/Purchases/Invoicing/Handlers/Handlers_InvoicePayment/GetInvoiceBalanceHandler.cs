using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_InvociePayment;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_InvoicePayment
{
    public class GetInvoiceBalanceHandler : IRequestHandler<GetInvoiceBalanceQuery, ResponseViewModel<GetInvoiceBalanceResponse>>
    {
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IGeneralRepository<SupplierPayment> _paymentRepository;
        private readonly IGeneralRepository<Supplier> _supplierRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetInvoiceBalanceHandler> _logger;

        public GetInvoiceBalanceHandler(
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IGeneralRepository<SupplierPayment> paymentRepository,
            IGeneralRepository<Supplier> supplierRepository,
            IMapper mapper,
            ILogger<GetInvoiceBalanceHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _supplierRepository = supplierRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<GetInvoiceBalanceResponse>> Handle(
            GetInvoiceBalanceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching balance for invoice {InvoiceId}", request.InvoiceId);

                var invoice = await _invoiceRepository
                    .Get(i => i.Id == request.InvoiceId)
                    .Select(i => new
                    {
                        i.Id,
                        i.InvoiceNumber,
                        i.InvoiceDate,
                        i.DueDate,
                        i.Subtotal,
                        i.TaxAmount,
                        i.TotalAmount,
                        i.DepositApplied,
                        i.AmountDue,
                        i.PaymentStatus,
                        i.SupplierId
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (invoice == null)
                {
                    _logger.LogWarning("Invoice not found with ID {InvoiceId}", request.InvoiceId);
                    throw new NotFoundException(
                        $"Invoice with ID {request.InvoiceId} not found",
                        FinanceErrorCode.NotFound);
                }

                var totalPaid = await _paymentRepository
                    .Get(p => p.InvoiceId == request.InvoiceId)
                    .SumAsync(p => p.Amount, cancellationToken);

                var supplier = await _supplierRepository
                    .Get(s => s.Id == invoice.SupplierId)
                    .Select(s => new SupplierInfoDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Email = s.Email,
                        Phone = s.Phone
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                int daysOverdue = 0;
                bool isOverdue = false;

                if (invoice.DueDate.HasValue && invoice.AmountDue > 0)
                {
                    daysOverdue = (DateTime.UtcNow.Date - invoice.DueDate.Value.Date).Days;
                    isOverdue = daysOverdue > 0;
                }

                var response = new GetInvoiceBalanceResponse
                {
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    InvoiceDate = invoice.InvoiceDate,
                    DueDate = invoice.DueDate,
                    Subtotal = invoice.Subtotal,
                    TaxAmount = invoice.TaxAmount,
                    TotalAmount = invoice.TotalAmount,
                    DepositApplied = invoice.DepositApplied,
                    TotalPaid = totalPaid,
                    AmountDue = invoice.AmountDue,
                    PaymentStatus = invoice.PaymentStatus,
                    DaysOverdue = isOverdue ? daysOverdue : 0,
                    IsOverdue = isOverdue,
                    Supplier = supplier ?? new SupplierInfoDto()
                };

                _logger.LogInformation(
                    "Successfully retrieved balance for invoice {InvoiceId}. AmountDue: {AmountDue}, IsOverdue: {IsOverdue}",
                    request.InvoiceId,
                    response.AmountDue,
                    response.IsOverdue);

                return ResponseViewModel<GetInvoiceBalanceResponse>.Success(
                    response,
                    "Invoice balance retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving balance for invoice {InvoiceId}", request.InvoiceId);
                throw new BusinessLogicException(
                    "An error occurred while retrieving invoice balance",
                    "Purchases",
                    FinanceErrorCode.DatabaseError);
            }
        }
    }
}
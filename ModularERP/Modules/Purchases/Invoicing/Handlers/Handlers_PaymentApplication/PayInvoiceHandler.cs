using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_PaymentApplication;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_PaymentApplication
{
    public class PayInvoiceHandler : IRequestHandler<PayInvoiceCommand, ResponseViewModel<PaymentApplicationSummaryDto>>
    {
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IGeneralRepository<SupplierPayment> _paymentRepository;
        private readonly IGeneralRepository<PaymentAllocation> _allocationRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PayInvoiceHandler> _logger;

        public PayInvoiceHandler(
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IGeneralRepository<SupplierPayment> paymentRepository,
            IGeneralRepository<PaymentAllocation> allocationRepository,
            IMapper mapper,
            ILogger<PayInvoiceHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _allocationRepository = allocationRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PaymentApplicationSummaryDto>> Handle(
            PayInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing payment for invoice {InvoiceId}", request.InvoiceId);

                // Get invoice
                var invoice = await _invoiceRepository.GetByID(request.InvoiceId);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice {InvoiceId} not found", request.InvoiceId);
                    return ResponseViewModel<PaymentApplicationSummaryDto>.Fail(
                        "Invoice not found",
                        FinanceErrorCode.NotFound);
                }

                // Validate payment amount
                if (request.Dto.Amount <= 0)
                {
                    _logger.LogWarning("Invalid payment amount: {Amount}", request.Dto.Amount);
                    return ResponseViewModel<PaymentApplicationSummaryDto>.Fail(
                        "Payment amount must be greater than zero",
                        FinanceErrorCode.BusinessLogicError);
                }

                if (request.Dto.Amount > invoice.AmountDue)
                {
                    _logger.LogWarning(
                        "Payment amount ({Amount}) exceeds invoice amount due ({AmountDue})",
                        request.Dto.Amount,
                        invoice.AmountDue);

                    return ResponseViewModel<PaymentApplicationSummaryDto>.Fail(
                        $"Payment amount ({request.Dto.Amount:N2}) exceeds invoice amount due ({invoice.AmountDue:N2})",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Validate invoice status
                if (invoice.PaymentStatus == PaymentStatus.PaidInFull)
                {
                    _logger.LogWarning("Invoice {InvoiceNumber} is already paid in full", invoice.InvoiceNumber);
                    return ResponseViewModel<PaymentApplicationSummaryDto>.Fail(
                        $"Invoice {invoice.InvoiceNumber} is already paid in full",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Create payment
                var payment = _mapper.Map<SupplierPayment>(request.Dto);
                payment.SupplierId = invoice.SupplierId;
                payment.InvoiceId = invoice.Id;
                payment.PaymentNumber = await GeneratePaymentNumber();

                await _paymentRepository.AddAsync(payment);

                // Create allocation
                var allocation = new PaymentAllocation
                {
                    PaymentId = payment.Id,
                    InvoiceId = invoice.Id,
                    AllocatedAmount = request.Dto.Amount,
                    AllocationDate = DateTime.UtcNow,
                    Notes = request.Dto.Notes
                };

                await _allocationRepository.AddAsync(allocation);

                // Update invoice
                invoice.AmountDue -= request.Dto.Amount;

                if (invoice.AmountDue == 0)
                {
                    invoice.PaymentStatus = PaymentStatus.PaidInFull;
                }
                else if (invoice.AmountDue < invoice.TotalAmount)
                {
                    invoice.PaymentStatus = PaymentStatus.PartiallyPaid;
                }

                await _paymentRepository.SaveChanges();

                _logger.LogInformation(
                    "Payment {PaymentNumber} created for invoice {InvoiceNumber}, Amount: {Amount}",
                    payment.PaymentNumber, invoice.InvoiceNumber, request.Dto.Amount);

                // Return summary
                var summary = new PaymentApplicationSummaryDto
                {
                    PaymentId = payment.Id,  // ✅ Fix
                    PaymentNumber = payment.PaymentNumber,
                    TotalAmount = payment.Amount,
                    AllocatedAmount = payment.AllocatedAmount,
                    UnallocatedAmount = payment.UnallocatedAmount,
                    AllocationsCount = 1,  // ✅ Fix
                    Allocations = new List<PaymentAllocationResponseDto>
                    {
                        new PaymentAllocationResponseDto
                        {
                            Id = allocation.Id,
                            PaymentId = payment.Id,
                            PaymentNumber = payment.PaymentNumber,
                            InvoiceId = invoice.Id,
                            InvoiceNumber = invoice.InvoiceNumber,
                            AllocatedAmount = request.Dto.Amount,
                            AllocationDate = allocation.AllocationDate,
                            Notes = allocation.Notes,
                            IsVoided = false
                        }
                    }
                };

                return ResponseViewModel<PaymentApplicationSummaryDto>.Success(
                    summary,
                    "Payment processed successfully");
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex, "Not found error in PayInvoiceHandler");
                return ResponseViewModel<PaymentApplicationSummaryDto>.Fail(
                    ex.Message,
                    FinanceErrorCode.NotFound);
            }
            catch (BusinessLogicException ex)
            {
                _logger.LogError(ex, "Business logic error in PayInvoiceHandler");
                return ResponseViewModel<PaymentApplicationSummaryDto>.Fail(
                    ex.Message,
                    FinanceErrorCode.BusinessLogicError);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error in PayInvoiceHandler");
                return ResponseViewModel<PaymentApplicationSummaryDto>.Fail(
                    "Failed to save payment to database. Please check data integrity.",
                    FinanceErrorCode.DatabaseError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in PayInvoiceHandler for invoice {InvoiceId}", request.InvoiceId);
                return ResponseViewModel<PaymentApplicationSummaryDto>.Fail(
                    $"An unexpected error occurred: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
            }
        }

        private async Task<string> GeneratePaymentNumber()
        {
            try
            {
                var lastPayment = await _paymentRepository
                    .GetAll()
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();

                if (lastPayment == null || string.IsNullOrEmpty(lastPayment.PaymentNumber))
                    return "PAY-00001";

                var parts = lastPayment.PaymentNumber.Split('-');
                if (parts.Length != 2 || !int.TryParse(parts[1], out int lastNumber))
                {
                    _logger.LogWarning("Invalid payment number format: {PaymentNumber}", lastPayment.PaymentNumber);
                    return "PAY-00001";
                }

                return $"PAY-{(lastNumber + 1):D5}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payment number");
                // Fallback to timestamp-based number
                return $"PAY-{DateTime.UtcNow.Ticks % 100000:D5}";
            }
        }
    }
}
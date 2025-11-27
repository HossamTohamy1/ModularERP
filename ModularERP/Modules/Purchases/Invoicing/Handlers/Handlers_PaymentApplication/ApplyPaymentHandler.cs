using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public class ApplyPaymentHandler : IRequestHandler<ApplyPaymentCommand, ResponseViewModel<PaymentApplicationSummaryDto>>
    {
        private readonly IGeneralRepository<SupplierPayment> _paymentRepository;
        private readonly IGeneralRepository<PaymentAllocation> _allocationRepository;
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ApplyPaymentHandler> _logger;

        public ApplyPaymentHandler(
            IGeneralRepository<SupplierPayment> paymentRepository,
            IGeneralRepository<PaymentAllocation> allocationRepository,
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IMapper mapper,
            ILogger<ApplyPaymentHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _allocationRepository = allocationRepository;
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PaymentApplicationSummaryDto>> Handle(
            ApplyPaymentCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Applying payment {PaymentId} to invoices", request.PaymentId);

            // Get payment
            var payment = await _paymentRepository.GetByID(request.PaymentId);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", request.PaymentId);
                throw new NotFoundException("Payment not found", FinanceErrorCode.NotFound);
            }

            // Validate payment status
            if (payment.Status == SupplierPaymentStatus.Void)
            {
                throw new BusinessLogicException(
                    "Cannot apply allocations to a voided payment",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // Calculate total allocation amount
            var totalAllocation = request.Dto.Allocations.Sum(a => a.AllocatedAmount);

            // Validate unallocated amount
            if (totalAllocation > payment.UnallocatedAmount)
            {
                throw new BusinessLogicException(
                    $"Total allocation amount ({totalAllocation}) exceeds unallocated amount ({payment.UnallocatedAmount})",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // Process each allocation
            foreach (var allocationDto in request.Dto.Allocations)
            {
                var invoice = await _invoiceRepository.GetByID(allocationDto.InvoiceId);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice {InvoiceId} not found", allocationDto.InvoiceId);
                    throw new NotFoundException($"Invoice {allocationDto.InvoiceId} not found", FinanceErrorCode.NotFound);
                }

                // Validate invoice belongs to same supplier
                if (invoice.SupplierId != payment.SupplierId)
                {
                    throw new BusinessLogicException(
                        $"Invoice {invoice.InvoiceNumber} does not belong to the payment supplier",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Validate allocation amount
                if (allocationDto.AllocatedAmount > invoice.AmountDue)
                {
                    throw new BusinessLogicException(
                        $"Allocation amount ({allocationDto.AllocatedAmount}) exceeds invoice amount due ({invoice.AmountDue})",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Create allocation
                var allocation = _mapper.Map<PaymentAllocation>(allocationDto);
                allocation.PaymentId = payment.Id;
                allocation.AllocationDate = DateTime.UtcNow;

                await _allocationRepository.AddAsync(allocation);

                await _allocationRepository.SaveChanges();

                // Update invoice amounts
                invoice.AmountDue -= allocationDto.AllocatedAmount;

                // Update payment status
                if (invoice.AmountDue == 0)
                {
                    invoice.PaymentStatus = PaymentStatus.PaidInFull;
                }
                else if (invoice.AmountDue < invoice.TotalAmount)
                {
                    invoice.PaymentStatus = PaymentStatus.PartiallyPaid;
                }

                _logger.LogInformation(
                    "Created allocation for payment {PaymentNumber} to invoice {InvoiceNumber}, Amount: {Amount}",
                    payment.PaymentNumber, invoice.InvoiceNumber, allocationDto.AllocatedAmount);
            }

            // Update payment amounts
            payment.AllocatedAmount += totalAllocation;
            payment.UnallocatedAmount -= totalAllocation;

            await _paymentRepository.SaveChanges();

            _logger.LogInformation("Successfully applied payment {PaymentId}", request.PaymentId);

            // Return summary
            var summary = await GetPaymentSummary(payment.Id, cancellationToken);
            return ResponseViewModel<PaymentApplicationSummaryDto>.Success(
                summary,
                "Payment applied successfully");
        }


        private async Task<PaymentApplicationSummaryDto> GetPaymentSummary(Guid paymentId, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository
                .Get(p => p.Id == paymentId)
                .Include(p => p.Allocations)
                    .ThenInclude(a => a.Invoice)
                .FirstOrDefaultAsync(cancellationToken);

            if (payment == null)
                throw new NotFoundException("Payment not found", FinanceErrorCode.NotFound);

            var allocations = payment.Allocations.Where(a => !a.IsVoided).ToList();

            var summary = new PaymentApplicationSummaryDto
            {
                PaymentId = payment.Id,
                PaymentNumber = payment.PaymentNumber,
                TotalAmount = payment.Amount,
                AllocatedAmount = payment.AllocatedAmount,
                UnallocatedAmount = payment.UnallocatedAmount,
                AllocationsCount = allocations.Count,
                Allocations = allocations
                    .Select(a => new PaymentAllocationResponseDto
                    {
                        Id = a.Id,
                        PaymentId = a.PaymentId,
                        PaymentNumber = payment.PaymentNumber,
                        InvoiceId = a.InvoiceId,
                        InvoiceNumber = a.Invoice.InvoiceNumber,
                        AllocatedAmount = a.AllocatedAmount,
                        AllocationDate = a.AllocationDate,
                        Notes = a.Notes,
                        AllocatedBy = a.CreatedById.ToString(),
                        IsVoided = a.IsVoided
                    })
                    .ToList()
            };

            return summary;
        }

    }

}

using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_PaymentApplication;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_PaymentApplication
{
    public class VoidAllocationHandler : IRequestHandler<VoidAllocationCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PaymentAllocation> _allocationRepository;
        private readonly IGeneralRepository<SupplierPayment> _paymentRepository;
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly ILogger<VoidAllocationHandler> _logger;

        public VoidAllocationHandler(
            IGeneralRepository<PaymentAllocation> allocationRepository,
            IGeneralRepository<SupplierPayment> paymentRepository,
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            ILogger<VoidAllocationHandler> logger)
        {
            _allocationRepository = allocationRepository;
            _paymentRepository = paymentRepository;
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            VoidAllocationCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Voiding allocation {AllocationId}", request.AllocationId);

            // Get allocation
            var allocation = await _allocationRepository.GetByID(request.AllocationId);
            if (allocation == null)
            {
                throw new NotFoundException("Allocation not found", FinanceErrorCode.NotFound);
            }

            if (allocation.IsVoided)
            {
                throw new BusinessLogicException(
                    "Allocation is already voided",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }

            // Get payment and invoice
            var payment = await _paymentRepository.GetByID(allocation.PaymentId);
            var invoice = await _invoiceRepository.GetByID(allocation.InvoiceId);

            if (payment == null || invoice == null)
            {
                throw new NotFoundException("Payment or invoice not found", FinanceErrorCode.NotFound);
            }

            // Void allocation
            allocation.IsVoided = true;
            allocation.VoidedAt = DateTime.UtcNow;
            allocation.VoidReason = request.Dto.VoidReason;

            // Update payment amounts
            payment.AllocatedAmount -= allocation.AllocatedAmount;
            payment.UnallocatedAmount += allocation.AllocatedAmount;

            // Update invoice amounts
            invoice.AmountDue += allocation.AllocatedAmount;

            if (invoice.AmountDue == invoice.TotalAmount)
            {
                invoice.PaymentStatus = "Unpaid";
            }
            else if (invoice.AmountDue > 0 && invoice.AmountDue < invoice.TotalAmount)
            {
                invoice.PaymentStatus = "PartiallyPaid";
            }

            await _allocationRepository.SaveChanges();

            _logger.LogInformation("Allocation {AllocationId} voided successfully", request.AllocationId);

            return ResponseViewModel<bool>.Success(true, "Allocation voided successfully");
        }
    }
}

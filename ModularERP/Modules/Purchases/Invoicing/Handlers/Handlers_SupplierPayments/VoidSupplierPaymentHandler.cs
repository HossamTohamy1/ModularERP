using MediatR;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_SupplierPayments;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_SupplierPayments
{
    public class VoidSupplierPaymentHandler : IRequestHandler<VoidSupplierPaymentCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<SupplierPayment> _paymentRepository;
        private readonly ILogger<VoidSupplierPaymentHandler> _logger;

        public VoidSupplierPaymentHandler(
            IGeneralRepository<SupplierPayment> paymentRepository,
            ILogger<VoidSupplierPaymentHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            VoidSupplierPaymentCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Voiding supplier payment: {PaymentId}", request.Id);

            // 1. Get payment with tracking
            var payment = await _paymentRepository.GetByIDWithTracking(request.Id);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found: {PaymentId}", request.Id);
                throw new NotFoundException(
                    $"Payment with ID {request.Id} not found",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            // 2. Check if already voided
            if (payment.IsVoid)
            {
                _logger.LogWarning("Payment is already voided: {PaymentId}", request.Id);
                throw new BusinessLogicException(
                    "Payment is already voided",
                    "Purchases");
            }

            // 3. Void the payment
            payment.IsVoid = true;
            payment.VoidedBy = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");//request.VoidedBy;
            payment.VoidedAt = DateTime.UtcNow;
            payment.VoidReason = request.VoidReason;
            payment.Status = "Void";
            payment.UpdatedAt = DateTime.UtcNow;

            // 4. Save changes
            await _paymentRepository.SaveChanges();

            _logger.LogInformation("Payment voided successfully: {PaymentId}", request.Id);

            return ResponseViewModel<bool>.Success(
                true,
                "Payment voided successfully");
        }
    }
}
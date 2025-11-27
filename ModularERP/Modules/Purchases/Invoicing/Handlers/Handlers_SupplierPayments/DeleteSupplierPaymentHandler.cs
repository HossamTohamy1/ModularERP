using MediatR;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_SupplierPayments;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_SupplierPayments
{
    public class DeleteSupplierPaymentHandler : IRequestHandler<DeleteSupplierPaymentCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<SupplierPayment> _paymentRepository;
        private readonly ILogger<DeleteSupplierPaymentHandler> _logger;

        public DeleteSupplierPaymentHandler(
            IGeneralRepository<SupplierPayment> paymentRepository,
            ILogger<DeleteSupplierPaymentHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeleteSupplierPaymentCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting supplier payment: {PaymentId}", request.Id);

            // 1. Get payment
            var payment = await _paymentRepository.GetByIDWithTracking(request.Id);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found: {PaymentId}", request.Id);
                throw new NotFoundException(
                    $"Payment with ID {request.Id} not found",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            // 2. Check if payment is void
            if (payment.IsVoid)
            {
                _logger.LogWarning("Cannot delete voided payment: {PaymentId}", request.Id);
                throw new BusinessLogicException(
                    "Cannot delete a voided payment",
                    "Purchases");
            }

            // 3. Check if payment is posted
            if (payment.Status == SupplierPaymentStatus.Posted)
            {
                _logger.LogWarning("Cannot delete posted payment: {PaymentId}", request.Id);
                throw new BusinessLogicException(
                    "Cannot delete a posted payment. Please void it instead.",
                    "Purchases");
            }

            // 4. Soft delete
            await _paymentRepository.Delete(request.Id);

            _logger.LogInformation("Payment deleted successfully: {PaymentId}", request.Id);

            return ResponseViewModel<bool>.Success(
                true,
                "Payment deleted successfully");
        }
    }
}

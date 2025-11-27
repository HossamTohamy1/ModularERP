using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentTerm;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Payment.Handlers.Handlers_PaymentTerm
{
    public class DeletePaymentTermHandler : IRequestHandler<DeletePaymentTermCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PaymentTerm> _repository;
        private readonly ILogger<DeletePaymentTermHandler> _logger;

        public DeletePaymentTermHandler(
            IGeneralRepository<PaymentTerm> repository,
            ILogger<DeletePaymentTermHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeletePaymentTermCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting Payment Term with ID: {Id}", request.Id);

                // Get existing entity
                var paymentTerm = await _repository.GetByIDWithTracking(request.Id);

                if (paymentTerm == null || paymentTerm.IsDeleted)
                {
                    _logger.LogWarning("Payment Term not found: {Id}", request.Id);
                    throw new NotFoundException(
                        $"Payment Term with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                // Soft delete
                await _repository.Delete(request.Id);

                _logger.LogInformation("Payment Term deleted successfully: {Id}", request.Id);

                return ResponseViewModel<bool>.Success(
                    true,
                    "Payment Term deleted successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Payment Term: {Id}", request.Id);
                throw new BusinessLogicException(
                    "An error occurred while deleting the Payment Term",
                    ex,
                    "Purchases");
            }
        }
    }
}

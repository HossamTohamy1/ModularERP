using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentMethod;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Payment.Handlers.Handlers_PaymentMethod
{
    public class DeletePaymentMethodCommandHandler : IRequestHandler<DeletePaymentMethodCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PaymentMethod> _repository;
        private readonly ILogger<DeletePaymentMethodCommandHandler> _logger;

        public DeletePaymentMethodCommandHandler(
            IGeneralRepository<PaymentMethod> repository,
            ILogger<DeletePaymentMethodCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeletePaymentMethodCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting payment method with ID: {Id}", request.Id);

                var paymentMethod = await _repository.GetByID(request.Id);

                if (paymentMethod == null)
                {
                    _logger.LogWarning("Payment method with ID {Id} not found", request.Id);
                    throw new NotFoundException(
                        $"Payment method with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                await _repository.Delete(request.Id);

                _logger.LogInformation("Successfully deleted payment method: {Name}", paymentMethod.Name);

                return ResponseViewModel<bool>.Success(
                    true,
                    "Payment method deleted successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting payment method with ID: {Id}", request.Id);
                return ResponseViewModel<bool>.Error(
                    "An error occurred while deleting payment method",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
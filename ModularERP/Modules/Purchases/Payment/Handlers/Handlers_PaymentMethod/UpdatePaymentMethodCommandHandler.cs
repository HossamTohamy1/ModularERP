using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentMethod;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentMethod;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Payment.Handlers.Handlers_PaymentMethod
{
    public class UpdatePaymentMethodCommandHandler : IRequestHandler<UpdatePaymentMethodCommand, ResponseViewModel<PaymentMethodDto>>
    {
        private readonly IGeneralRepository<PaymentMethod> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdatePaymentMethodCommandHandler> _logger;

        public UpdatePaymentMethodCommandHandler(
            IGeneralRepository<PaymentMethod> repository,
            IMapper mapper,
            ILogger<UpdatePaymentMethodCommandHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PaymentMethodDto>> Handle(
            UpdatePaymentMethodCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating payment method with ID: {Id}", request.Id);

                var existingPaymentMethod = await _repository.GetByID(request.Id);

                if (existingPaymentMethod == null)
                {
                    _logger.LogWarning("Payment method with ID {Id} not found", request.Id);
                    throw new NotFoundException(
                        $"Payment method with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                _mapper.Map(request, existingPaymentMethod);
                existingPaymentMethod.Code = request.Code.ToUpper();
                existingPaymentMethod.UpdatedAt = DateTime.UtcNow;

                await _repository.Update(existingPaymentMethod);

                var result = _mapper.Map<PaymentMethodDto>(existingPaymentMethod);

                _logger.LogInformation("Successfully updated payment method: {Name}", result.Name);

                return ResponseViewModel<PaymentMethodDto>.Success(
                    result,
                    "Payment method updated successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating payment method with ID: {Id}", request.Id);
                return ResponseViewModel<PaymentMethodDto>.Error(
                    "An error occurred while updating payment method",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
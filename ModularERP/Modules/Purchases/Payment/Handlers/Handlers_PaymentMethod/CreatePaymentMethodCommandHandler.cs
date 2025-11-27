using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentMethod;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentMethod;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Payment.Handlers.Handlers_PaymentMethod
{
    public class CreatePaymentMethodCommandHandler : IRequestHandler<CreatePaymentMethodCommand, ResponseViewModel<PaymentMethodDto>>
    {
        private readonly IGeneralRepository<PaymentMethod> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreatePaymentMethodCommandHandler> _logger;

        public CreatePaymentMethodCommandHandler(
            IGeneralRepository<PaymentMethod> repository,
            IMapper mapper,
            ILogger<CreatePaymentMethodCommandHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PaymentMethodDto>> Handle(
            CreatePaymentMethodCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new payment method: {Name}", request.Name);

                var paymentMethod = _mapper.Map<PaymentMethod>(request);
                paymentMethod.Code = request.Code.ToUpper();

                await _repository.AddAsync(paymentMethod);
                await _repository.SaveChanges();

                var result = _mapper.Map<PaymentMethodDto>(paymentMethod);

                _logger.LogInformation("Successfully created payment method with ID: {Id}", result.Id);

                return ResponseViewModel<PaymentMethodDto>.Success(
                    result,
                    "Payment method created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating payment method: {Name}", request.Name);
                return ResponseViewModel<PaymentMethodDto>.Error(
                    "An error occurred while creating payment method",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
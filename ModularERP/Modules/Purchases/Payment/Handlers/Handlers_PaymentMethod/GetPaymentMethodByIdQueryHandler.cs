using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentMethod;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Modules.Purchases.Payment.Qeuries.Quries_PaymentMethod;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Payment.Handlers.Handlers_PaymentMethod
{
    public class GetPaymentMethodByIdQueryHandler : IRequestHandler<GetPaymentMethodByIdQuery, ResponseViewModel<PaymentMethodDto>>
    {
        private readonly IGeneralRepository<PaymentMethod> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPaymentMethodByIdQueryHandler> _logger;

        public GetPaymentMethodByIdQueryHandler(
            IGeneralRepository<PaymentMethod> repository,
            IMapper mapper,
            ILogger<GetPaymentMethodByIdQueryHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PaymentMethodDto>> Handle(
            GetPaymentMethodByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching payment method with ID: {Id}", request.Id);

                var paymentMethod = await _repository
                    .Get(x => x.Id == request.Id)
                    .ProjectTo<PaymentMethodDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                if (paymentMethod == null)
                {
                    _logger.LogWarning("Payment method with ID {Id} not found", request.Id);
                    throw new NotFoundException(
                        $"Payment method with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Successfully fetched payment method: {Name}", paymentMethod.Name);

                return ResponseViewModel<PaymentMethodDto>.Success(
                    paymentMethod,
                    "Payment method retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching payment method with ID: {Id}", request.Id);
                return ResponseViewModel<PaymentMethodDto>.Error(
                    "An error occurred while fetching payment method",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}

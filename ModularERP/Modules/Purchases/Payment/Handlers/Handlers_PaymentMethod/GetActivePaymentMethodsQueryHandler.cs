using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentMethod;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Modules.Purchases.Payment.Qeuries.Quries_PaymentMethod;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Payment.Handlers.Handlers_PaymentMethod
{
    public class GetActivePaymentMethodsQueryHandler : IRequestHandler<GetActivePaymentMethodsQuery, ResponseViewModel<List<PaymentMethodDto>>>
    {
        private readonly IGeneralRepository<PaymentMethod> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetActivePaymentMethodsQueryHandler> _logger;

        public GetActivePaymentMethodsQueryHandler(
            IGeneralRepository<PaymentMethod> repository,
            IMapper mapper,
            ILogger<GetActivePaymentMethodsQueryHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<PaymentMethodDto>>> Handle(
            GetActivePaymentMethodsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching active payment methods");

                var paymentMethods = await _repository
                    .Get(x => x.IsActive)
                    .OrderBy(x => x.Name)
                    .ProjectTo<PaymentMethodDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Successfully fetched {Count} active payment methods", paymentMethods.Count);

                return ResponseViewModel<List<PaymentMethodDto>>.Success(
                    paymentMethods,
                    $"Retrieved {paymentMethods.Count} active payment methods successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching active payment methods");
                return ResponseViewModel<List<PaymentMethodDto>>.Error(
                    "An error occurred while fetching active payment methods",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }

}

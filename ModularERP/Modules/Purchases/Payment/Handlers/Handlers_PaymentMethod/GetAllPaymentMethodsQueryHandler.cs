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
    public class GetAllPaymentMethodsQueryHandler : IRequestHandler<GetAllPaymentMethodsQuery, ResponseViewModel<List<PaymentMethodDto>>>
    {
        private readonly IGeneralRepository<PaymentMethod> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllPaymentMethodsQueryHandler> _logger;

        public GetAllPaymentMethodsQueryHandler(
            IGeneralRepository<PaymentMethod> repository,
            IMapper mapper,
            ILogger<GetAllPaymentMethodsQueryHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<PaymentMethodDto>>> Handle(
            GetAllPaymentMethodsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching all payment methods");

                var query = _repository.GetAll();

                if (request.IsActive.HasValue)
                {
                    query = query.Where(x => x.IsActive == request.IsActive.Value);
                }

                var paymentMethods = await query
                    .OrderBy(x => x.Name)
                    .ProjectTo<PaymentMethodDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Successfully fetched {Count} payment methods", paymentMethods.Count);

                return ResponseViewModel<List<PaymentMethodDto>>.Success(
                    paymentMethods,
                    $"Retrieved {paymentMethods.Count} payment methods successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching payment methods");
                return ResponseViewModel<List<PaymentMethodDto>>.Error(
                    "An error occurred while fetching payment methods",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}

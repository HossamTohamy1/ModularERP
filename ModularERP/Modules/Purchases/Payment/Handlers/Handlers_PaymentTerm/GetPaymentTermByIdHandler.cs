using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentTerm;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Modules.Purchases.Payment.Qeuries.Queries_PaymentTerm;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Payment.Handlers.Handlers_PaymentTerm
{
    public class GetPaymentTermByIdHandler : IRequestHandler<GetPaymentTermByIdQuery, ResponseViewModel<PaymentTermResponseDto>>
    {
        private readonly IGeneralRepository<PaymentTerm> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPaymentTermByIdHandler> _logger;

        public GetPaymentTermByIdHandler(
            IGeneralRepository<PaymentTerm> repository,
            IMapper mapper,
            ILogger<GetPaymentTermByIdHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PaymentTermResponseDto>> Handle(
            GetPaymentTermByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching Payment Term with ID: {Id}", request.Id);

                // Get entity without Include (using projection)
                var paymentTerm = await _repository.GetByID(request.Id);

                if (paymentTerm == null || paymentTerm.IsDeleted)
                {
                    _logger.LogWarning("Payment Term not found: {Id}", request.Id);
                    throw new NotFoundException(
                        $"Payment Term with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                // Use AutoMapper projection
                var response = _mapper.Map<PaymentTermResponseDto>(paymentTerm);

                _logger.LogInformation("Payment Term retrieved successfully: {Id}", request.Id);

                return ResponseViewModel<PaymentTermResponseDto>.Success(
                    response,
                    "Payment Term retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Payment Term: {Id}", request.Id);
                throw new BusinessLogicException(
                    "An error occurred while fetching the Payment Term",
                    ex,
                    "Purchases");
            }
        }
    }
}

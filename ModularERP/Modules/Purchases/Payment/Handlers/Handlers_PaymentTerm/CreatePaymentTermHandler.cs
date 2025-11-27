using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentTerm;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentTerm;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Payment.Handlers.Handlers_PaymentTerm
{
    public class CreatePaymentTermHandler : IRequestHandler<CreatePaymentTermCommand, ResponseViewModel<PaymentTermResponseDto>>
    {
        private readonly IGeneralRepository<PaymentTerm> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreatePaymentTermHandler> _logger;

        public CreatePaymentTermHandler(
            IGeneralRepository<PaymentTerm> repository,
            IMapper mapper,
            ILogger<CreatePaymentTermHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PaymentTermResponseDto>> Handle(
            CreatePaymentTermCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating Payment Term: {Name}", request.Dto.Name);

                // Check if name already exists
                var exists = await _repository.AnyAsync(
                    x => x.Name.ToLower() == request.Dto.Name.ToLower() && !x.IsDeleted,
                    cancellationToken);

                if (exists)
                {
                    _logger.LogWarning("Payment Term with name {Name} already exists", request.Dto.Name);
                    throw new BusinessLogicException(
                        $"Payment Term with name '{request.Dto.Name}' already exists",
                        "Purchases",
                        FinanceErrorCode.DuplicateRecord);
                }

                // Map DTO to Entity
                var paymentTerm = _mapper.Map<PaymentTerm>(request.Dto);
                paymentTerm.Id = Guid.NewGuid();
                paymentTerm.CreatedAt = DateTime.UtcNow;

                // Save to database
                await _repository.AddAsync(paymentTerm);
                await _repository.SaveChanges();

                _logger.LogInformation("Payment Term created successfully with ID: {Id}", paymentTerm.Id);

                // Map to Response DTO using projection
                var response = _mapper.Map<PaymentTermResponseDto>(paymentTerm);

                return ResponseViewModel<PaymentTermResponseDto>.Success(
                    response,
                    "Payment Term created successfully");
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Payment Term: {Name}", request.Dto.Name);
                throw new BusinessLogicException(
                    "An error occurred while creating the Payment Term",
                    ex,
                    "Purchases");
            }
        }
    }
}
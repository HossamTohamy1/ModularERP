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
    public class UpdatePaymentTermHandler : IRequestHandler<UpdatePaymentTermCommand, ResponseViewModel<PaymentTermResponseDto>>
    {
        private readonly IGeneralRepository<PaymentTerm> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdatePaymentTermHandler> _logger;

        public UpdatePaymentTermHandler(
            IGeneralRepository<PaymentTerm> repository,
            IMapper mapper,
            ILogger<UpdatePaymentTermHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PaymentTermResponseDto>> Handle(
            UpdatePaymentTermCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating Payment Term with ID: {Id}", request.Id);

                // Get existing entity
                var existingPaymentTerm = await _repository.GetByIDWithTracking(request.Id);

                if (existingPaymentTerm == null || existingPaymentTerm.IsDeleted)
                {
                    _logger.LogWarning("Payment Term not found: {Id}", request.Id);
                    throw new NotFoundException(
                        $"Payment Term with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                // Check duplicate name (if name is being updated)
                if (!string.IsNullOrEmpty(request.Dto.Name) &&
                    request.Dto.Name.ToLower() != existingPaymentTerm.Name.ToLower())
                {
                    var duplicateExists = await _repository.AnyAsync(
                        x => x.Name.ToLower() == request.Dto.Name.ToLower() &&
                             x.Id != request.Id &&
                             !x.IsDeleted,
                        cancellationToken);

                    if (duplicateExists)
                    {
                        _logger.LogWarning("Payment Term with name {Name} already exists", request.Dto.Name);
                        throw new BusinessLogicException(
                            $"Payment Term with name '{request.Dto.Name}' already exists",
                            "Purchases",
                            FinanceErrorCode.DuplicateRecord);
                    }
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(request.Dto.Name))
                    existingPaymentTerm.Name = request.Dto.Name;

                if (request.Dto.Days.HasValue)
                    existingPaymentTerm.Days = request.Dto.Days.Value;

                if (request.Dto.Description != null)
                    existingPaymentTerm.Description = request.Dto.Description;

                if (request.Dto.IsActive.HasValue)
                    existingPaymentTerm.IsActive = request.Dto.IsActive.Value;

                existingPaymentTerm.UpdatedAt = DateTime.UtcNow;

                // Save changes
                await _repository.SaveChanges();

                _logger.LogInformation("Payment Term updated successfully: {Id}", request.Id);

                // Map to Response DTO
                var response = _mapper.Map<PaymentTermResponseDto>(existingPaymentTerm);

                return ResponseViewModel<PaymentTermResponseDto>.Success(
                    response,
                    "Payment Term updated successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Payment Term: {Id}", request.Id);
                throw new BusinessLogicException(
                    "An error occurred while updating the Payment Term",
                    ex,
                    "Purchases");
            }
        }
    }
}
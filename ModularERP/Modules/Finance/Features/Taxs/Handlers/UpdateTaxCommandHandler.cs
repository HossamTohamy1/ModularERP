using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Taxs.Commands;
using ModularERP.Modules.Finance.Features.Taxs.DTO;
using ModularERP.Modules.Finance.Features.Taxs.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.Taxs.Handlers
{
    public class UpdateTaxCommandHandler : IRequestHandler<UpdateTaxCommand, ResponseViewModel<TaxResponseDto>>
    {
        private readonly IGeneralRepository<Tax> _taxRepository;
        private readonly IMapper _mapper;

        public UpdateTaxCommandHandler(IGeneralRepository<Tax> taxRepository, IMapper mapper)
        {
            _taxRepository = taxRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<TaxResponseDto>> Handle(UpdateTaxCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Business Logic: Get existing tax
                var existingTax = await _taxRepository.GetByIDWithTracking(request.UpdateTaxDto.Id);

                if (existingTax == null)
                {
                    throw new NotFoundException(
                        $"Tax with ID '{request.UpdateTaxDto.Id}' not found",
                        FinanceErrorCode.NotFound);
                }

                // Business Logic: Check if code is unique (excluding current tax)
                var duplicateCode = await _taxRepository
                    .Get(t => t.Code == request.UpdateTaxDto.Code
                        && t.Id != request.UpdateTaxDto.Id && !t.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (duplicateCode != null)
                {
                    throw new BusinessLogicException(
                        $"Tax with code '{request.UpdateTaxDto.Code}' already exists",
                        "Finance",
                        FinanceErrorCode.DuplicateEntity);
                }

                // Business Logic: Check if name is unique (excluding current tax)
                var duplicateName = await _taxRepository
                    .Get(t => t.Name == request.UpdateTaxDto.Name
                        && t.Id != request.UpdateTaxDto.Id && !t.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (duplicateName != null)
                {
                    throw new BusinessLogicException(
                        $"Tax with name '{request.UpdateTaxDto.Name}' already exists",
                        "Finance",
                        FinanceErrorCode.DuplicateEntity);
                }

                // Business Logic: Validate rate range
                if (request.UpdateTaxDto.Rate < 0 || request.UpdateTaxDto.Rate > 100)
                {
                    throw new BusinessLogicException(
                        "Tax rate must be between 0 and 100",
                        "Finance",
                        FinanceErrorCode.ValidationError);
                }

                _mapper.Map(request.UpdateTaxDto, existingTax);
                existingTax.UpdatedAt = DateTime.UtcNow;

                await _taxRepository.SaveChanges();

                var response = _mapper.Map<TaxResponseDto>(existingTax);
                return ResponseViewModel<TaxResponseDto>.Success(response, "Tax updated successfully");
            }
            catch (Exception ex) when (!(ex is NotFoundException || ex is BusinessLogicException))
            {
                throw new BusinessLogicException(
                    $"Error updating tax: {ex.Message}",
                    "Finance",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}

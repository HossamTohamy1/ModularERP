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
    public class CreateTaxCommandHandler : IRequestHandler<CreateTaxCommand, ResponseViewModel<TaxResponseDto>>
    {
        private readonly IGeneralRepository<Tax> _taxRepository;
        private readonly IMapper _mapper;

        public CreateTaxCommandHandler(IGeneralRepository<Tax> taxRepository, IMapper mapper)
        {
            _taxRepository = taxRepository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<TaxResponseDto>> Handle(CreateTaxCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Business Logic: Check if code is unique
                var existingTaxByCode = await _taxRepository
                    .Get(t => t.Code == request.CreateTaxDto.Code && !t.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingTaxByCode != null)
                {
                    throw new BusinessLogicException(
                        $"Tax with code '{request.CreateTaxDto.Code}' already exists",
                        "Finance",
                        FinanceErrorCode.DuplicateEntity);
                }

                // Business Logic: Check if name is unique
                var existingTaxByName = await _taxRepository
                    .Get(t => t.Name == request.CreateTaxDto.Name && !t.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingTaxByName != null)
                {
                    throw new BusinessLogicException(
                        $"Tax with name '{request.CreateTaxDto.Name}' already exists",
                        "Finance",
                        FinanceErrorCode.DuplicateEntity);
                }

                // Business Logic: Validate rate range
                if (request.CreateTaxDto.Rate < 0 || request.CreateTaxDto.Rate > 100)
                {
                    throw new BusinessLogicException(
                        "Tax rate must be between 0 and 100",
                        "Finance",
                        FinanceErrorCode.ValidationError);
                }

                var tax = _mapper.Map<Tax>(request.CreateTaxDto);
                await _taxRepository.AddAsync(tax);
                await _taxRepository.SaveChanges();

                var response = _mapper.Map<TaxResponseDto>(tax);
                return ResponseViewModel<TaxResponseDto>.Success(response, "Tax created successfully");
            }
            catch (Exception ex) when (!(ex is BusinessLogicException))
            {
                throw new BusinessLogicException(
                    $"Error creating tax: {ex.Message}",
                    "Finance",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}

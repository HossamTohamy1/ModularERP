using AutoMapper;
using FluentValidation;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Companys.Commands;
using ModularERP.Modules.Finance.Features.Companys.DTO;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Shared.Interfaces;
using ValidationException = ModularERP.Common.Exceptions.ValidationException;

namespace ModularERP.Modules.Finance.Features.Companys.Handlers
{
    public class UpdateCompanyHandler : IRequestHandler<UpdateCompanyCommand, ResponseViewModel<CompanyResponseDto>>
    {
        private readonly IGeneralRepository<Company> _companyRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdateCompanyCommand> _validator;
        private readonly ILogger<UpdateCompanyHandler> _logger;

        public UpdateCompanyHandler(
            IGeneralRepository<Company> companyRepository,
            IMapper mapper,
            IValidator<UpdateCompanyCommand> validator,
            ILogger<UpdateCompanyHandler> logger)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        public async Task<ResponseViewModel<CompanyResponseDto>> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating company with ID: {CompanyId}", request.CompanyDto.Id);

            try
            {
                // Validate command
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var validationErrors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                    _logger.LogWarning("Validation failed for updating company: {@ValidationErrors}", validationErrors);
                    throw new ValidationException("Validation failed", validationErrors, "Finance");
                }

                // Find existing company
                var existingCompany = await _companyRepository.GetByIDWithTracking(request.CompanyDto.Id);
                if (existingCompany == null)
                {
                    _logger.LogWarning("Company with ID {CompanyId} not found", request.CompanyDto.Id);
                    throw new NotFoundException($"Company with ID '{request.CompanyDto.Id}' not found", FinanceErrorCode.NotFound);
                }

                // Check if another company with same name exists (excluding current company)
                var duplicateNameExists = await _companyRepository.AnyAsync(c =>
                    c.Name == request.CompanyDto.Name && c.Id != request.CompanyDto.Id);

                if (duplicateNameExists)
                {
                    _logger.LogWarning("Another company with name {CompanyName} already exists", request.CompanyDto.Name);
                    throw new BusinessLogicException($"Another company with name '{request.CompanyDto.Name}' already exists", "Finance", FinanceErrorCode.TreasuryAlreadyExists);
                }

                // Update properties
                existingCompany.Name = request.CompanyDto.Name;
                existingCompany.CurrencyCode = request.CompanyDto.CurrencyCode;
                existingCompany.UpdatedAt = DateTime.UtcNow;

                await _companyRepository.SaveChanges();

                // Map to response DTO
                var responseDto = _mapper.Map<CompanyResponseDto>(existingCompany);

                _logger.LogInformation("Successfully updated company with ID: {CompanyId}", existingCompany.Id);

                return ResponseViewModel<CompanyResponseDto>.Success(
                    responseDto,
                    "Company updated successfully"
                );
            }
            catch (Exception ex) when (!(ex is BaseApplicationException))
            {
                _logger.LogError(ex, "Unexpected error occurred while updating company with ID: {CompanyId}", request.CompanyDto.Id);
                throw new BusinessLogicException("An error occurred while updating the company", "Finance", FinanceErrorCode.InternalServerError);
            }
        }
    }
}

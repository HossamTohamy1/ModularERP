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
    public class CreateCompanyHandler : IRequestHandler<CreateCompanyCommand, ResponseViewModel<CompanyResponseDto>>
    {
        private readonly IGeneralRepository<Company> _companyRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateCompanyCommand> _validator;
        private readonly ILogger<CreateCompanyHandler> _logger;

        public CreateCompanyHandler(
            IGeneralRepository<Company> companyRepository,
            IMapper mapper,
            IValidator<CreateCompanyCommand> validator,
            ILogger<CreateCompanyHandler> logger)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        public async Task<ResponseViewModel<CompanyResponseDto>> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new company with name: {CompanyName}", request.CompanyDto.Name);

            try
            {
                // Validate command
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var validationErrors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                    _logger.LogWarning("Validation failed for creating company: {@ValidationErrors}", validationErrors);
                    throw new ValidationException("Validation failed", validationErrors, "Finance");
                }

                // Check if company with same name already exists
                var existingCompany = await _companyRepository.AnyAsync(c => c.Name == request.CompanyDto.Name);
                if (existingCompany)
                {
                    _logger.LogWarning("Company with name {CompanyName} already exists", request.CompanyDto.Name);
                    throw new BusinessLogicException($"Company with name '{request.CompanyDto.Name}' already exists", "Finance", FinanceErrorCode.TreasuryAlreadyExists);
                }

                // Map DTO to entity
                var company = _mapper.Map<Company>(request.CompanyDto);
                company.Id = Guid.NewGuid();
                company.CreatedAt = DateTime.UtcNow;

                // Add to database
                await _companyRepository.AddAsync(company);
                await _companyRepository.SaveChanges();

                // Map to response DTO
                var responseDto = _mapper.Map<CompanyResponseDto>(company);

                _logger.LogInformation("Successfully created company with ID: {CompanyId}", company.Id);

                return ResponseViewModel<CompanyResponseDto>.Success(
                    responseDto,
                    "Company created successfully"
                );
            }
            catch (Exception ex) when (!(ex is BaseApplicationException))
            {
                _logger.LogError(ex, "Unexpected error occurred while creating company");
                throw new BusinessLogicException("An error occurred while creating the company", "Finance", FinanceErrorCode.InternalServerError);
            }
        }
    }

}

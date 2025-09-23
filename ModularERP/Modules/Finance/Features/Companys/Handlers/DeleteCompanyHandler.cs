using FluentValidation;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Companys.Commands;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Shared.Interfaces;
using ValidationException = ModularERP.Common.Exceptions.ValidationException;

namespace ModularERP.Modules.Finance.Features.Companys.Handlers
{
    public class DeleteCompanyHandler : IRequestHandler<DeleteCompanyCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Company> _companyRepository;
        private readonly IValidator<DeleteCompanyCommand> _validator;
        private readonly ILogger<DeleteCompanyHandler> _logger;

        public DeleteCompanyHandler(
            IGeneralRepository<Company> companyRepository,
            IValidator<DeleteCompanyCommand> validator,
            ILogger<DeleteCompanyHandler> logger)
        {
            _companyRepository = companyRepository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting company with ID: {CompanyId}", request.Id);

            try
            {
                // Validate command
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var validationErrors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                    _logger.LogWarning("Validation failed for deleting company: {@ValidationErrors}", validationErrors);
                    throw new ValidationException("Validation failed", validationErrors, "Finance");
                }

                // Find existing company
                var existingCompany = await _companyRepository.GetByID(request.Id);
                if (existingCompany == null)
                {
                    _logger.LogWarning("Company with ID {CompanyId} not found", request.Id);
                    throw new NotFoundException($"Company with ID '{request.Id}' not found", FinanceErrorCode.NotFound);
                }

                // Check if company has related data (optional business rule)
                var hasTreasuries = existingCompany.Treasuries?.Any() == true;
                var hasBankAccounts = existingCompany.BankAccounts?.Any() == true;
                var hasVouchers = existingCompany.Vouchers?.Any() == true;

                if (hasTreasuries || hasBankAccounts || hasVouchers)
                {
                    _logger.LogWarning("Cannot delete company {CompanyId} as it has related data", request.Id);
                    throw new BusinessLogicException("Cannot delete company as it has related treasuries, bank accounts, or vouchers", "Finance", FinanceErrorCode.TreasuryHasVouchers);
                }

                // Soft delete
                await _companyRepository.Delete(request.Id);

                _logger.LogInformation("Successfully deleted company with ID: {CompanyId}", request.Id);

                return ResponseViewModel<bool>.Success(
                    true,
                    "Company deleted successfully"
                );
            }
            catch (Exception ex) when (!(ex is BaseApplicationException))
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting company with ID: {CompanyId}", request.Id);
                throw new BusinessLogicException("An error occurred while deleting the company", "Finance", FinanceErrorCode.InternalServerError);
            }
        }
    }
}

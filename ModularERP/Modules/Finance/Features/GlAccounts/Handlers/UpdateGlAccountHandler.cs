using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.GlAccounts.Commands;
using ModularERP.Modules.Finance.Features.GlAccounts.DTO;
using ModularERP.Modules.Finance.Features.GlAccounts.Models;
using ModularERP.Shared.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Handlers
{
    public class UpdateGlAccountHandler : IRequestHandler<UpdateGlAccountCommand, ResponseViewModel<GlAccountResponseDto>>
    {
        private readonly IGeneralRepository<GlAccount> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger = Log.ForContext<UpdateGlAccountHandler>();

        public UpdateGlAccountHandler(IGeneralRepository<GlAccount> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseViewModel<GlAccountResponseDto>> Handle(UpdateGlAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("==> Starting GLAccount update process");
                _logger.Debug("Request received: {@Request}", request.GlAccount);

                // Check if GLAccount exists
                _logger.Debug("Fetching GLAccount with ID: {Id}", request.GlAccount.Id);
                var existingAccount = await _repository.GetByIDWithTracking(request.GlAccount.Id);

                if (existingAccount == null)
                {
                    _logger.Warning("GLAccount update failed - Account with ID {Id} not found", request.GlAccount.Id);
                    throw new NotFoundException(
                        $"GLAccount with ID '{request.GlAccount.Id}' not found",
                        FinanceErrorCode.NotFound);
                }
                _logger.Debug("GLAccount found in DB: {@ExistingAccount}", existingAccount);

                // Check for duplicate code (excluding current account)
                _logger.Debug("Checking for duplicate GLAccount code: {Code} in Company {CompanyId}",
                    request.GlAccount.Code, request.GlAccount.CompanyId);

                var duplicateCodeExists = await _repository.AnyAsync(x =>
                    x.Code == request.GlAccount.Code &&
                    x.CompanyId == request.GlAccount.CompanyId &&
                    x.Id != request.GlAccount.Id);

                if (duplicateCodeExists)
                {
                    _logger.Warning("Duplicate code detected: {Code} already exists in Company {CompanyId}",
                        request.GlAccount.Code, request.GlAccount.CompanyId);

                    throw new BusinessLogicException(
                        $"GLAccount with code '{request.GlAccount.Code}' already exists in this company",
                        "Finance",
                        FinanceErrorCode.DuplicateRecord);
                }

                // Update entity
                _logger.Debug("Mapping updated fields from DTO to Entity...");
                _mapper.Map(request.GlAccount, existingAccount);
                existingAccount.UpdatedAt = DateTime.UtcNow;

                _logger.Debug("Entity after mapping: {@UpdatedEntity}", existingAccount);

                _logger.Debug("Saving changes to database...");
                await _repository.SaveChanges();
                _logger.Information("Changes saved successfully for GLAccount ID: {Id}", existingAccount.Id);

                // Get updated entity with company info
                _logger.Debug("Fetching updated GLAccount with Company info for ID: {Id}", existingAccount.Id);
                var updatedAccount = await _repository.GetByCompanyId(request.GlAccount.CompanyId)
                    .Where(x => x.Id == existingAccount.Id)
                    .Select(x => new GlAccount
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Name = x.Name,
                        Type = x.Type,
                        IsLeaf = x.IsLeaf,
                        CompanyId = x.CompanyId,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        Company = new ModularERP.Modules.Finance.Features.Companys.Models.Company
                        {
                            Name = x.Company.Name
                        }
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                _logger.Debug("Updated GLAccount fetched: {@UpdatedAccount}", updatedAccount);

                var responseDto = _mapper.Map<GlAccountResponseDto>(updatedAccount);
                _logger.Debug("Mapped response DTO: {@ResponseDto}", responseDto);

                _logger.Information("GLAccount updated successfully with ID: {Id}, Code: {Code}",
                    existingAccount.Id, existingAccount.Code);

                return ResponseViewModel<GlAccountResponseDto>.Success(responseDto, "GLAccount updated successfully");
            }
            catch (NotFoundException ex)
            {
                _logger.Warning(ex, "NotFoundException occurred while updating GLAccount with ID: {Id}", request.GlAccount.Id);
                throw;
            }
            catch (BusinessLogicException ex)
            {
                _logger.Warning(ex, "BusinessLogicException occurred while updating GLAccount with ID: {Id}", request.GlAccount.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error occurred while updating GLAccount with ID: {Id}", request.GlAccount.Id);
                throw new BusinessLogicException(
                    "An error occurred while updating the GLAccount",
                    "Finance",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}

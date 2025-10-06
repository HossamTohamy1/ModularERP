using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.Commands;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Shared.Interfaces;
using ModularERP.Common.Services;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Finance.Features.Companys.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Handlers
{
    public class CreateBankAccountHandler
        : IRequestHandler<CreateBankAccountCommand, ResponseViewModel<BankAccountCreatedDto>>
    {
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateBankAccountHandler> _logger;
        private readonly ITenantService _tenantService;
        private readonly IMasterDbService _masterDbService;
        private readonly FinanceDbContext _context;

        public CreateBankAccountHandler(
            IGeneralRepository<BankAccount> bankAccountRepository,
            IMapper mapper,
            ILogger<CreateBankAccountHandler> logger,
            ITenantService tenantService,
            IMasterDbService masterDbService,
            FinanceDbContext context)
        {
            _bankAccountRepository = bankAccountRepository;
            _mapper = mapper;
            _logger = logger;
            _tenantService = tenantService;
            _masterDbService = masterDbService;
            _context = context;
        }

        public async Task<ResponseViewModel<BankAccountCreatedDto>> Handle(
            CreateBankAccountCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Get current tenant info
                var currentTenantId = _tenantService.GetCurrentTenantId();
                if (string.IsNullOrEmpty(currentTenantId))
                {
                    _logger.LogWarning("No tenant context available");
                    return ResponseViewModel<BankAccountCreatedDto>.Error(
                        "No tenant context available",
                        FinanceErrorCode.InvalidData);
                }

                _logger.LogInformation("Received CreateBankAccountCommand for TenantId: {TenantId}, CompanyId: {CompanyId}",
                                     currentTenantId, request?.BankAccount?.CompanyId);

                // Validate input data
                if (request?.BankAccount == null)
                {
                    _logger.LogWarning("Bank account data is null");
                    return ResponseViewModel<BankAccountCreatedDto>.Error(
                        "Bank account data is null",
                        FinanceErrorCode.InvalidData);
                }

                var requestedCompanyId = request.BankAccount.CompanyId;

                // Validate required fields
                if (string.IsNullOrEmpty(request.BankAccount.BankName) ||
                    string.IsNullOrEmpty(request.BankAccount.AccountNumber))
                {
                    _logger.LogWarning("Missing required fields: BankName or AccountNumber");
                    return ResponseViewModel<BankAccountCreatedDto>.Error(
                        "Bank name and account number are required",
                        FinanceErrorCode.InvalidData);
                }

                // Check if company exists in current tenant database - if not, create it
                await EnsureCompanyExistsInTenantAsync(requestedCompanyId, currentTenantId, cancellationToken);

                // Check for existing bank account
                _logger.LogInformation("Checking if bank account already exists for CompanyId: {CompanyId}",
                                       requestedCompanyId);

                var existingBankAccount = await _bankAccountRepository
                    .Get(ba => ba.CompanyId == requestedCompanyId &&
                               ba.AccountNumber == request.BankAccount.AccountNumber &&
                               ba.BankName == request.BankAccount.BankName)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingBankAccount != null)
                {
                    _logger.LogWarning("Bank account already exists for CompanyId: {CompanyId}, BankName: {BankName}, AccountNumber: {AccountNumber}",
                                       requestedCompanyId,
                                       request.BankAccount.BankName,
                                       request.BankAccount.AccountNumber);

                    return ResponseViewModel<BankAccountCreatedDto>.Error(
                        "Bank account with this account number already exists for the bank and company",
                        FinanceErrorCode.BankAccountAlreadyExists);
                }

                // Map to entity
                BankAccount bankAccount;
                try
                {
                    bankAccount = _mapper.Map<BankAccount>(request.BankAccount);
                }
                catch (Exception mappingEx)
                {
                    _logger.LogError(mappingEx, "Mapping error while creating bank account");
                    return ResponseViewModel<BankAccountCreatedDto>.Error(
                        $"Mapping error: {mappingEx.Message}",
                        FinanceErrorCode.InternalServerError);
                }

                // Set system values
                bankAccount.Id = Guid.NewGuid();
                bankAccount.CreatedAt = DateTime.UtcNow;
                bankAccount.CompanyId = requestedCompanyId; // Keep the requested CompanyId

                _logger.LogInformation("Adding new bank account: {BankName} / {AccountNumber} for Company: {CompanyId}",
                                       bankAccount.BankName,
                                       bankAccount.AccountNumber,
                                       requestedCompanyId);

                // Save the bank account
                await _bankAccountRepository.AddAsync(bankAccount);
                await _bankAccountRepository.SaveChanges();

                _logger.LogInformation("Bank account {BankAccountId} created successfully", bankAccount.Id);

                // Map to result DTO
                var result = _mapper.Map<BankAccountCreatedDto>(bankAccount);

                var glAccountExists = await _context.GlAccounts
                    .AnyAsync(g => g.Id == request.BankAccount.JournalAccountId && !g.IsDeleted, cancellationToken);

                if (!glAccountExists)
                {
                    return ResponseViewModel<BankAccountCreatedDto>.Error(
                        "Invalid GL Account selected",
                        FinanceErrorCode.InvalidData);
                }


                return ResponseViewModel<BankAccountCreatedDto>.Success(
                    result,
                    "Bank account created successfully");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while creating bank account. Inner exception: {InnerException}",
                               dbEx.InnerException?.Message);
                return ResponseViewModel<BankAccountCreatedDto>.Error(
                    $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}",
                    FinanceErrorCode.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating bank account");
                return ResponseViewModel<BankAccountCreatedDto>.Error(
                    $"An error occurred while creating the bank account: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
            }
        }

        private async Task EnsureCompanyExistsInTenantAsync(Guid companyId, string tenantId, CancellationToken cancellationToken)
        {
            try
            {
                var companyExists = await _context.Companies
                    .AnyAsync(c => c.Id == companyId && !c.IsDeleted, cancellationToken);

                if (companyExists)
                {
                    _logger.LogDebug("Company {CompanyId} already exists in tenant database", companyId);
                    return;
                }

                _logger.LogInformation("Company {CompanyId} not found in tenant database, checking master database", companyId);

                var masterCompany = await _masterDbService.GetCompanyAsync(companyId);
                if (masterCompany == null)
                {
                    _logger.LogError("Company {CompanyId} not found in master database", companyId);
                    throw new InvalidOperationException($"Company {companyId} not found in master database");
                }

                var tenantCompany = new Company
                {
                    Id = masterCompany.Id,
                    Name = masterCompany.Name,
                    CurrencyCode = masterCompany.CurrencyCode,
     
                    CreatedAt = masterCompany.CreatedAt,
                    IsDeleted = false
                };

                _context.Companies.Add(tenantCompany);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully created company {CompanyId} ({CompanyName}) in tenant {TenantId}",
                                     companyId, masterCompany.Name, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ensure company {CompanyId} exists in tenant {TenantId}",
                               companyId, tenantId);
                throw;
            }
        }
    }
}
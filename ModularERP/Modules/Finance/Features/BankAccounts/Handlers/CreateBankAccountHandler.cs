using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.Commands;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Handlers
{
    public class CreateBankAccountHandler : IRequestHandler<CreateBankAccountCommand, ResponseViewModel<BankAccountCreatedDto>>
    {
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;
        private readonly IMapper _mapper;

        public CreateBankAccountHandler(IGeneralRepository<BankAccount> bankAccountRepository, IMapper mapper)
        {
            _bankAccountRepository = bankAccountRepository;
            _mapper = mapper;
        }

        // Add this to your CreateBankAccountHandler.cs for debugging:

        public async Task<ResponseViewModel<BankAccountCreatedDto>> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Add null check for debugging
                if (request?.BankAccount == null)
                {
                    return ResponseViewModel<BankAccountCreatedDto>.Error(
                        "Bank account data is null",
                        FinanceErrorCode.InvalidData);
                }

                // Check for required fields
                if (string.IsNullOrEmpty(request.BankAccount.BankName) ||
                    string.IsNullOrEmpty(request.BankAccount.AccountNumber))
                {
                    return ResponseViewModel<BankAccountCreatedDto>.Error(
                        "Bank name and account number are required",
                        FinanceErrorCode.InvalidData);
                }

                var existingBankAccount = await _bankAccountRepository
                    .Get(ba => ba.CompanyId == request.BankAccount.CompanyId &&
                              ba.AccountNumber == request.BankAccount.AccountNumber &&
                              ba.BankName == request.BankAccount.BankName)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingBankAccount != null)
                {
                    return ResponseViewModel<BankAccountCreatedDto>.Error(
                        "Bank account with this account number already exists for the bank and company",
                        FinanceErrorCode.BankAccountAlreadyExists);
                }

                // Try mapping with error handling
                BankAccount bankAccount;
                try
                {
                    bankAccount = _mapper.Map<BankAccount>(request.BankAccount);
                }
                catch (Exception mappingEx)
                {
                    return ResponseViewModel<BankAccountCreatedDto>.Error(
                        $"Mapping error: {mappingEx.Message}",
                        FinanceErrorCode.InternalServerError);
                }

                bankAccount.Id = Guid.NewGuid();
                bankAccount.CreatedAt = DateTime.UtcNow;

                await _bankAccountRepository.AddAsync(bankAccount);
                await _bankAccountRepository.SaveChanges();

                var result = _mapper.Map<BankAccountCreatedDto>(bankAccount);
                return ResponseViewModel<BankAccountCreatedDto>.Success(result, "Bank account created successfully");
            }
            catch (Exception ex)
            {
                // Log the actual exception details for debugging
                return ResponseViewModel<BankAccountCreatedDto>.Error(
                    $"An error occurred while creating the bank account: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}

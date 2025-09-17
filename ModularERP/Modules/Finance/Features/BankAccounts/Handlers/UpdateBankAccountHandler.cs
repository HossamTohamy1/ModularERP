using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.Commands;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Handlers
{
    public class UpdateBankAccountHandler : IRequestHandler<UpdateBankAccountCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;
        private readonly IMapper _mapper;

        public UpdateBankAccountHandler(IGeneralRepository<BankAccount> bankAccountRepository, IMapper mapper)
        {
            _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ResponseViewModel<bool>> Handle(UpdateBankAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate request
                if (request?.BankAccount == null)
                {
                    return ResponseViewModel<bool>.Error(
                        "Bank account data is required",
                        FinanceErrorCode.InvalidData);
                }

                if (request.BankAccount.Id == Guid.Empty)
                {
                    return ResponseViewModel<bool>.Error(
                        "Bank account ID is required",
                        FinanceErrorCode.InvalidData);
                }

                // Find existing bank account
                var bankAccount = await _bankAccountRepository
                    .Get(ba => ba.Id == request.BankAccount.Id && !ba.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (bankAccount == null)
                {
                    return ResponseViewModel<bool>.Error(
                        "Bank account not found",
                        FinanceErrorCode.BankAccountNotFound);
                }

                // Check for duplicate (different account with same details)
                var existingBankAccount = await _bankAccountRepository
                    .Get(ba => ba.CompanyId == request.BankAccount.CompanyId &&
                              ba.AccountNumber == request.BankAccount.AccountNumber &&
                              ba.BankName == request.BankAccount.BankName &&
                              ba.Id != request.BankAccount.Id &&
                              !ba.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingBankAccount != null)
                {
                    return ResponseViewModel<bool>.Error(
                        "Bank account with this account number already exists for the bank and company",
                        FinanceErrorCode.BankAccountAlreadyExists);
                }

                // Update entity
                _mapper.Map(request.BankAccount, bankAccount);
                bankAccount.UpdatedAt = DateTime.UtcNow;

                await _bankAccountRepository.Update(bankAccount);
                return ResponseViewModel<bool>.Success(true, "Bank account updated successfully");
            }
            catch (Exception ex)
            {
                return ResponseViewModel<bool>.Error(
                    $"An error occurred while updating the bank account: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}

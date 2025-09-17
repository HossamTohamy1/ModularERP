using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.Commands;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Handlers
{
    public class DeleteBankAccountHandler : IRequestHandler<DeleteBankAccountCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;

        public DeleteBankAccountHandler(IGeneralRepository<BankAccount> bankAccountRepository)
        {
            _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteBankAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate request
                if (request.Id == Guid.Empty)
                {
                    return ResponseViewModel<bool>.Error(
                        "Bank account ID is required",
                        FinanceErrorCode.InvalidData);
                }

                // Find bank account
                var bankAccount = await _bankAccountRepository
                    .Get(ba => ba.Id == request.Id && !ba.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (bankAccount == null)
                {
                    return ResponseViewModel<bool>.Error(
                        "Bank account not found",
                        FinanceErrorCode.BankAccountNotFound);
                }

                // Check for associated vouchers
                var vouchersCount = await _bankAccountRepository
                    .Get(ba => ba.Id == request.Id)
                    .SelectMany(ba => ba.Vouchers)
                    .CountAsync(cancellationToken);

                if (vouchersCount > 0)
                {
                    return ResponseViewModel<bool>.Error(
                        "Cannot delete bank account that has associated vouchers",
                        FinanceErrorCode.BankAccountHasVouchers);
                }

                // Soft delete
                bankAccount.IsDeleted = true;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                await _bankAccountRepository.Update(bankAccount);
                return ResponseViewModel<bool>.Success(true, "Bank account deleted successfully");
            }
            catch (Exception ex)
            {
                return ResponseViewModel<bool>.Error(
                    $"An error occurred while deleting the bank account: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}

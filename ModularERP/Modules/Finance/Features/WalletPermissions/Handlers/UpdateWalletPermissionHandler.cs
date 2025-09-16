using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.WalletPermissions.Commands;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.WalletPermissions.Handlers
{
    public class UpdateWalletPermissionHandler : IRequestHandler<UpdateWalletPermissionCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Treasury> _treasuryRepository;
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;
        private readonly ILogger<UpdateWalletPermissionHandler> _logger;

        public UpdateWalletPermissionHandler(
            IGeneralRepository<Treasury> treasuryRepository,
            IGeneralRepository<BankAccount> bankAccountRepository,
            ILogger<UpdateWalletPermissionHandler> logger)
        {
            _treasuryRepository = treasuryRepository;
            _bankAccountRepository = bankAccountRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(UpdateWalletPermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = request.Permissions.UserId.ToString();

                if (request.WalletType.Equals("treasury", StringComparison.OrdinalIgnoreCase))
                {
                    var treasury = await _treasuryRepository.GetByIDWithTracking(request.WalletId);
                    if (treasury == null)
                        return ResponseViewModel<bool>.Error("Treasury not found", FinanceErrorCode.NotFound);

                    (treasury.DepositAcl, treasury.WithdrawAcl) =
                        UpdateAcls(treasury.DepositAcl, treasury.WithdrawAcl, userId, request.Permissions.CanDeposit, request.Permissions.CanWithdraw);

                    await _treasuryRepository.Update(treasury);
                }
                else if (request.WalletType.Equals("bankaccount", StringComparison.OrdinalIgnoreCase))
                {
                    var bankAccount = await _bankAccountRepository.GetByIDWithTracking(request.WalletId);
                    if (bankAccount == null)
                        return ResponseViewModel<bool>.Error("Bank Account not found", FinanceErrorCode.NotFound);

                    (bankAccount.DepositAcl, bankAccount.WithdrawAcl) =
                        UpdateAcls(bankAccount.DepositAcl, bankAccount.WithdrawAcl, userId, request.Permissions.CanDeposit, request.Permissions.CanWithdraw);

                    await _bankAccountRepository.Update(bankAccount);
                }
                else
                {
                    return ResponseViewModel<bool>.Error("Invalid wallet type", FinanceErrorCode.ValidationError);
                }

                return ResponseViewModel<bool>.Success(true, "Wallet permissions updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating wallet permissions");
                return ResponseViewModel<bool>.Error("Error updating wallet permissions", FinanceErrorCode.InternalServerError);
            }
        }

        private (string depositAcl, string withdrawAcl) UpdateAcls(
            string? currentDeposit,
            string? currentWithdraw,
            string userId,
            bool canDeposit,
            bool canWithdraw)
        {
            var depositAcl = string.IsNullOrWhiteSpace(currentDeposit)
                ? new List<string>()
                : currentDeposit.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            var withdrawAcl = string.IsNullOrWhiteSpace(currentWithdraw)
                ? new List<string>()
                : currentWithdraw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            // Deposit
            if (canDeposit && !depositAcl.Contains(userId))
                depositAcl.Add(userId);
            else if (!canDeposit && depositAcl.Contains(userId))
                depositAcl.Remove(userId);

            // Withdraw
            if (canWithdraw && !withdrawAcl.Contains(userId))
                withdrawAcl.Add(userId);
            else if (!canWithdraw && withdrawAcl.Contains(userId))
                withdrawAcl.Remove(userId);

            return (string.Join(",", depositAcl), string.Join(",", withdrawAcl));
        }
    }
}

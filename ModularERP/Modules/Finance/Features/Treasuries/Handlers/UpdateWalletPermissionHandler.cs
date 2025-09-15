using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.Treasuries.Handlers
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
                if (request.WalletType.ToLower() == "treasury")
                {
                    var treasury = await _treasuryRepository.GetByIDWithTracking(request.WalletId);
                    if (treasury == null)
                        return ResponseViewModel<bool>.Error("Treasury not found", FinanceErrorCode.NotFound);

                    _treasuryRepository.UpdateInclude(treasury, nameof(treasury.DepositAcl), nameof(treasury.WithdrawAcl));
                }
                else if (request.WalletType.ToLower() == "bankaccount")
                {
                    var bankAccount = await _bankAccountRepository.GetByIDWithTracking(request.WalletId);
                    if (bankAccount == null)
                        return ResponseViewModel<bool>.Error("Bank Account not found", FinanceErrorCode.NotFound);

                    bankAccount.DepositAcl = request.Permissions.DepositAcl;
                    bankAccount.WithdrawAcl = request.Permissions.WithdrawAcl;

                    _bankAccountRepository.UpdateInclude(bankAccount, nameof(bankAccount.DepositAcl), nameof(bankAccount.WithdrawAcl));
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
    }
}

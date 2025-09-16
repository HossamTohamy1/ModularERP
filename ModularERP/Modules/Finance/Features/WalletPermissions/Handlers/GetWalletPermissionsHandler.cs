using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.WalletPermissions.DTO;
using ModularERP.Modules.Finance.Features.WalletPermissions.Queries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.WalletPermissions.Handlers
{
    public class GetWalletPermissionsHandler : IRequestHandler<GetWalletPermissionsQuery, ResponseViewModel<WalletPermissionDto>>
    {
        private readonly IGeneralRepository<Treasury> _treasuryRepository;
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;
        private readonly ILogger<GetWalletPermissionsHandler> _logger;

        public GetWalletPermissionsHandler(
            IGeneralRepository<Treasury> treasuryRepository,
            IGeneralRepository<BankAccount> bankAccountRepository,
            ILogger<GetWalletPermissionsHandler> logger)
        {
            _treasuryRepository = treasuryRepository;
            _bankAccountRepository = bankAccountRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<WalletPermissionDto>> Handle(GetWalletPermissionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                WalletPermissionDto walletDto = new();

                if (request.WalletType.ToLower() == "treasury")
                {
                    var treasury = await _treasuryRepository.GetByID(request.WalletId);
                    if (treasury == null)
                        return ResponseViewModel<WalletPermissionDto>.Error("Treasury not found", FinanceErrorCode.NotFound);

                    walletDto = new WalletPermissionDto
                    {
                        WalletId = treasury.Id,
                        WalletType = "Treasury",
                        WalletName = treasury.Name,
                        DepositAcl = treasury.DepositAcl,
                        WithdrawAcl = treasury.WithdrawAcl,
                        CurrencyCode = treasury.CurrencyCode,
                        Status = treasury.Status.ToString()
                    };
                }
                else if (request.WalletType.ToLower() == "bankaccount")
                {
                    var bankAccount = await _bankAccountRepository.GetByID(request.WalletId);
                    if (bankAccount == null)
                        return ResponseViewModel<WalletPermissionDto>.Error("Bank Account not found", FinanceErrorCode.NotFound);

                    walletDto = new WalletPermissionDto
                    {
                        WalletId = bankAccount.Id,
                        WalletType = "BankAccount",
                        WalletName = bankAccount.Name,
                        DepositAcl = bankAccount.DepositAcl,
                        WithdrawAcl = bankAccount.WithdrawAcl,
                        CurrencyCode = bankAccount.CurrencyCode,
                        Status = bankAccount.Status.ToString()
                    };
                }
                else
                {
                    return ResponseViewModel<WalletPermissionDto>.Error("Invalid wallet type", FinanceErrorCode.ValidationError);
                }

                return ResponseViewModel<WalletPermissionDto>.Success(walletDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet permissions");
                return ResponseViewModel<WalletPermissionDto>.Error("Error getting wallet permissions", FinanceErrorCode.InternalServerError);
            }
        }
    }
}

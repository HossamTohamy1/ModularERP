using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.WalletPermissions.Commands;
using ModularERP.Shared.Interfaces;
using System.Text.Json;

namespace ModularERP.Modules.Finance.Features.WalletPermissions.Handlers
{
    public class DeleteWalletPermissionHandler : IRequestHandler<DeleteWalletPermissionCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Treasury> _treasuryRepository;
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;
        private readonly ILogger<DeleteWalletPermissionHandler> _logger;

        public DeleteWalletPermissionHandler(
            IGeneralRepository<Treasury> treasuryRepository,
            IGeneralRepository<BankAccount> bankAccountRepository,
            ILogger<DeleteWalletPermissionHandler> logger)
        {
            _treasuryRepository = treasuryRepository;
            _bankAccountRepository = bankAccountRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteWalletPermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.WalletType.ToLower() == "treasury")
                {
                    var treasury = await _treasuryRepository.GetByIDWithTracking(request.WalletId);
                    if (treasury == null)
                        return ResponseViewModel<bool>.Error("Treasury not found", FinanceErrorCode.NotFound);

                    // Remove user from ACL
                    var depositAcl = JsonSerializer.Deserialize<Dictionary<string, object>>(treasury.DepositAcl) ?? new Dictionary<string, object>();
                    var withdrawAcl = JsonSerializer.Deserialize<Dictionary<string, object>>(treasury.WithdrawAcl) ?? new Dictionary<string, object>();

                    if (depositAcl.ContainsKey("users"))
                    {
                        var users = JsonSerializer.Deserialize<List<string>>(depositAcl["users"].ToString() ?? "[]") ?? new List<string>();
                        users.Remove(request.UserId.ToString());
                        depositAcl["users"] = users;
                    }

                    if (withdrawAcl.ContainsKey("users"))
                    {
                        var users = JsonSerializer.Deserialize<List<string>>(withdrawAcl["users"].ToString() ?? "[]") ?? new List<string>();
                        users.Remove(request.UserId.ToString());
                        withdrawAcl["users"] = users;
                    }

                    treasury.DepositAcl = JsonSerializer.Serialize(depositAcl);
                    treasury.WithdrawAcl = JsonSerializer.Serialize(withdrawAcl);

                    _treasuryRepository.UpdateInclude(treasury, nameof(treasury.DepositAcl), nameof(treasury.WithdrawAcl));
                }
                else if (request.WalletType.ToLower() == "bankaccount")
                {
                    var bankAccount = await _bankAccountRepository.GetByIDWithTracking(request.WalletId);
                    if (bankAccount == null)
                        return ResponseViewModel<bool>.Error("Bank Account not found", FinanceErrorCode.NotFound);

                    // Similar logic for bank account
                    var depositAcl = JsonSerializer.Deserialize<Dictionary<string, object>>(bankAccount.DepositAcl) ?? new Dictionary<string, object>();
                    var withdrawAcl = JsonSerializer.Deserialize<Dictionary<string, object>>(bankAccount.WithdrawAcl) ?? new Dictionary<string, object>();

                    if (depositAcl.ContainsKey("users"))
                    {
                        var users = JsonSerializer.Deserialize<List<string>>(depositAcl["users"].ToString() ?? "[]") ?? new List<string>();
                        users.Remove(request.UserId.ToString());
                        depositAcl["users"] = users;
                    }

                    if (withdrawAcl.ContainsKey("users"))
                    {
                        var users = JsonSerializer.Deserialize<List<string>>(withdrawAcl["users"].ToString() ?? "[]") ?? new List<string>();
                        users.Remove(request.UserId.ToString());
                        withdrawAcl["users"] = users;
                    }

                    bankAccount.DepositAcl = JsonSerializer.Serialize(depositAcl);
                    bankAccount.WithdrawAcl = JsonSerializer.Serialize(withdrawAcl);

                    _bankAccountRepository.UpdateInclude(bankAccount, nameof(bankAccount.DepositAcl), nameof(bankAccount.WithdrawAcl));
                }

                return ResponseViewModel<bool>.Success(true, "Wallet permission removed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting wallet permission");
                return ResponseViewModel<bool>.Error("Error deleting wallet permission", FinanceErrorCode.InternalServerError);
            }
        }
    }
}

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
    public class AddWalletPermissionHandler : IRequestHandler<AddWalletPermissionCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Treasury> _treasuryRepository;
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;
        private readonly ILogger<AddWalletPermissionHandler> _logger;

        public AddWalletPermissionHandler(
            IGeneralRepository<Treasury> treasuryRepository,
            IGeneralRepository<BankAccount> bankAccountRepository,
            ILogger<AddWalletPermissionHandler> logger)
        {
            _treasuryRepository = treasuryRepository;
            _bankAccountRepository = bankAccountRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(AddWalletPermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.WalletType.ToLower() == "treasury")
                {
                    var treasury = await _treasuryRepository.GetByIDWithTracking(request.WalletId);
                    if (treasury == null)
                        return ResponseViewModel<bool>.Error("Treasury not found", FinanceErrorCode.NotFound);

                    treasury.DepositAcl = UpdateAcl(treasury.DepositAcl, request.UserId, request.CanDeposit);
                    treasury.WithdrawAcl = UpdateAcl(treasury.WithdrawAcl, request.UserId, request.CanWithdraw);

                    await _treasuryRepository.Update(treasury);
                }
                else if (request.WalletType.ToLower() == "bankaccount")
                {
                    var bankAccount = await _bankAccountRepository.GetByIDWithTracking(request.WalletId);
                    if (bankAccount == null)
                        return ResponseViewModel<bool>.Error("Bank Account not found", FinanceErrorCode.NotFound);

                    bankAccount.DepositAcl = UpdateAcl(bankAccount.DepositAcl, request.UserId, request.CanDeposit);
                    bankAccount.WithdrawAcl = UpdateAcl(bankAccount.WithdrawAcl, request.UserId, request.CanWithdraw);

                    await _bankAccountRepository.Update(bankAccount);
                }
                else
                {
                    return ResponseViewModel<bool>.Error("Invalid wallet type", FinanceErrorCode.ValidationError);
                }

                return ResponseViewModel<bool>.Success(true, "Wallet permission added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding wallet permission");
                return ResponseViewModel<bool>.Error("Error adding wallet permission", FinanceErrorCode.InternalServerError);
            }
        }

        private string UpdateAcl(string? aclJson, Guid userId, bool canDoAction)
        {
            var acl = JsonSerializer.Deserialize<Dictionary<string, object>>(aclJson ?? "{}")
                      ?? new Dictionary<string, object>();

            if (canDoAction)
            {
                if (!acl.ContainsKey("users"))
                    acl["users"] = new List<string>();

                var users = JsonSerializer.Deserialize<List<string>>(acl["users"].ToString() ?? "[]")
                            ?? new List<string>();

                if (!users.Contains(userId.ToString()))
                    users.Add(userId.ToString());

                acl["users"] = users;
            }

            return JsonSerializer.Serialize(acl);
        }
    }
}

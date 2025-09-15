using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Shared.Interfaces;
using System.Text.Json;

namespace ModularERP.Modules.Finance.Features.Treasuries.Handlers
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

                    // Update ACL logic here - parse JSON and add user permissions
                    var depositAcl = JsonSerializer.Deserialize<Dictionary<string, object>>(treasury.DepositAcl) ?? new Dictionary<string, object>();
                    var withdrawAcl = JsonSerializer.Deserialize<Dictionary<string, object>>(treasury.WithdrawAcl) ?? new Dictionary<string, object>();

                    if (request.CanDeposit)
                    {
                        if (!depositAcl.ContainsKey("users"))
                            depositAcl["users"] = new List<string>();

                        var users = JsonSerializer.Deserialize<List<string>>(depositAcl["users"].ToString() ?? "[]") ?? new List<string>();
                        if (!users.Contains(request.UserId.ToString()))
                            users.Add(request.UserId.ToString());

                        depositAcl["users"] = users;
                    }

                    if (request.CanWithdraw)
                    {
                        if (!withdrawAcl.ContainsKey("users"))
                            withdrawAcl["users"] = new List<string>();

                        var users = JsonSerializer.Deserialize<List<string>>(withdrawAcl["users"].ToString() ?? "[]") ?? new List<string>();
                        if (!users.Contains(request.UserId.ToString()))
                            users.Add(request.UserId.ToString());

                        withdrawAcl["users"] = users;
                    }

                    treasury.DepositAcl = JsonSerializer.Serialize(depositAcl);
                    treasury.WithdrawAcl = JsonSerializer.Serialize(withdrawAcl);

                    await _treasuryRepository.Update(treasury);
                }
                else if (request.WalletType.ToLower() == "bankaccount")
                {
                    var bankAccount = await _bankAccountRepository.GetByIDWithTracking(request.WalletId);
                    if (bankAccount == null)
                        return ResponseViewModel<bool>.Error("Bank Account not found", FinanceErrorCode.NotFound);

                    // Similar ACL update logic for bank account
                    var depositAcl = JsonSerializer.Deserialize<Dictionary<string, object>>(bankAccount.DepositAcl) ?? new Dictionary<string, object>();
                    var withdrawAcl = JsonSerializer.Deserialize<Dictionary<string, object>>(bankAccount.WithdrawAcl) ?? new Dictionary<string, object>();

                    if (request.CanDeposit)
                    {
                        if (!depositAcl.ContainsKey("users"))
                            depositAcl["users"] = new List<string>();

                        var users = JsonSerializer.Deserialize<List<string>>(depositAcl["users"].ToString() ?? "[]") ?? new List<string>();
                        if (!users.Contains(request.UserId.ToString()))
                            users.Add(request.UserId.ToString());

                        depositAcl["users"] = users;
                    }

                    if (request.CanWithdraw)
                    {
                        if (!withdrawAcl.ContainsKey("users"))
                            withdrawAcl["users"] = new List<string>();

                        var users = JsonSerializer.Deserialize<List<string>>(withdrawAcl["users"].ToString() ?? "[]") ?? new List<string>();
                        if (!users.Contains(request.UserId.ToString()))
                            users.Add(request.UserId.ToString());

                        withdrawAcl["users"] = users;
                    }

                    bankAccount.DepositAcl = JsonSerializer.Serialize(depositAcl);
                    bankAccount.WithdrawAcl = JsonSerializer.Serialize(withdrawAcl);

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
    }
}
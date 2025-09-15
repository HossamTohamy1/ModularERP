using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.Treasuries.Queries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.Treasuries.Handlers
{
    public class GetUserWalletsHandler : IRequestHandler<GetUserWalletsQuery, ResponseViewModel<List<WalletPermissionDto>>>
    {
        private readonly IGeneralRepository<Treasury> _treasuryRepository;
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;
        private readonly ILogger<GetUserWalletsHandler> _logger;

        public GetUserWalletsHandler(
            IGeneralRepository<Treasury> treasuryRepository,
            IGeneralRepository<BankAccount> bankAccountRepository,
            ILogger<GetUserWalletsHandler> logger)
        {
            _treasuryRepository = treasuryRepository;
            _bankAccountRepository = bankAccountRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<WalletPermissionDto>>> Handle(GetUserWalletsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = new List<WalletPermissionDto>();

                // Get user's treasuries
                if (string.IsNullOrEmpty(request.WalletType) || request.WalletType.ToLower() == "treasury")
                {
                    var treasuries = _treasuryRepository.GetAll().Where(t =>
                        t.DepositAcl.Contains(request.UserId.ToString()) ||
                        t.WithdrawAcl.Contains(request.UserId.ToString())).ToList();

                    result.AddRange(treasuries.Select(t => new WalletPermissionDto
                    {
                        WalletId = t.Id,
                        WalletType = "Treasury",
                        WalletName = t.Name,
                        DepositAcl = t.DepositAcl,
                        WithdrawAcl = t.WithdrawAcl,
                        CurrencyCode = t.CurrencyCode,
                        Status = t.Status.ToString()
                    }));
                }

                // Get user's bank accounts
                if (string.IsNullOrEmpty(request.WalletType) || request.WalletType.ToLower() == "bankaccount")
                {
                    var bankAccounts = _bankAccountRepository.GetAll().Where(b =>
                        b.DepositAcl.Contains(request.UserId.ToString()) ||
                        b.WithdrawAcl.Contains(request.UserId.ToString())).ToList();

                    result.AddRange(bankAccounts.Select(b => new WalletPermissionDto
                    {
                        WalletId = b.Id,
                        WalletType = "BankAccount",
                        WalletName = b.Name,
                        DepositAcl = b.DepositAcl,
                        WithdrawAcl = b.WithdrawAcl,
                        CurrencyCode = b.CurrencyCode,
                        Status = b.Status.ToString()
                    }));
                }

                return ResponseViewModel<List<WalletPermissionDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user wallets");
                return ResponseViewModel<List<WalletPermissionDto>>.Error("Error getting user wallets", FinanceErrorCode.InternalServerError);
            }
        }
    }
}

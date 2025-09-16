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
    public class GetAllWalletsHandler : IRequestHandler<GetAllWalletsQuery, ResponseViewModel<List<WalletPermissionDto>>>
    {
        private readonly IGeneralRepository<Treasury> _treasuryRepository;
        private readonly IGeneralRepository<BankAccount> _bankAccountRepository;
        private readonly ILogger<GetAllWalletsHandler> _logger;

        public GetAllWalletsHandler(
            IGeneralRepository<Treasury> treasuryRepository,
            IGeneralRepository<BankAccount> bankAccountRepository,
            ILogger<GetAllWalletsHandler> logger)
        {
            _treasuryRepository = treasuryRepository;
            _bankAccountRepository = bankAccountRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<WalletPermissionDto>>> Handle(GetAllWalletsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = new List<WalletPermissionDto>();

                // Get treasuries
                if (string.IsNullOrEmpty(request.WalletType) || request.WalletType.ToLower() == "treasury")
                {
                    var treasuriesQuery = _treasuryRepository.GetAll();

                    if (request.CompanyId.HasValue)
                        treasuriesQuery = treasuriesQuery.Where(t => t.CompanyId == request.CompanyId.Value);

                    var treasuries = treasuriesQuery.ToList();

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

                // Get bank accounts
                if (string.IsNullOrEmpty(request.WalletType) || request.WalletType.ToLower() == "bankaccount")
                {
                    var bankAccountsQuery = _bankAccountRepository.GetAll();

                    if (request.CompanyId.HasValue)
                        bankAccountsQuery = bankAccountsQuery.Where(b => b.CompanyId == request.CompanyId.Value);

                    var bankAccounts = bankAccountsQuery.ToList();

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
                _logger.LogError(ex, "Error getting all wallets");
                return ResponseViewModel<List<WalletPermissionDto>>.Error("Error getting all wallets", FinanceErrorCode.InternalServerError);
            }
        }
    }
}


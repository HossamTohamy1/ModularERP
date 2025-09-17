using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Modules.Finance.Features.GlAccounts.Models;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.Service.Interface
{
    public class IncomeVoucherService : IIncomeVoucherService
    {
        private readonly IGeneralRepository<Treasury> _treasuryRepo;
        private readonly IGeneralRepository<BankAccount> _bankAccountRepo;
        private readonly IGeneralRepository<GlAccount> _glAccountRepo;
        private readonly IGeneralRepository<Voucher> _voucherRepo;

        public IncomeVoucherService(
            IGeneralRepository<Treasury> treasuryRepo,
            IGeneralRepository<BankAccount> bankAccountRepo,
            IGeneralRepository<GlAccount> glAccountRepo,
            IGeneralRepository<Voucher> voucherRepo)
        {
            _treasuryRepo = treasuryRepo;
            _bankAccountRepo = bankAccountRepo;
            _glAccountRepo = glAccountRepo;
            _voucherRepo = voucherRepo;
        }

        public async Task<bool> ValidateWalletPermissionAsync(Guid walletId, string walletType, Guid userId, string operation)
        {
            if (walletType == "Treasury")
            {
                var treasury = await _treasuryRepo.GetByID(walletId);
                if (treasury == null) return false;

                var acl = operation == "Deposit" ? treasury.DepositAcl : treasury.WithdrawAcl;
                return acl == "Everyone"; // Simplified for now
            }

            if (walletType == "BankAccount")
            {
                var bankAccount = await _bankAccountRepo.GetByID(walletId);
                if (bankAccount == null) return false;

                var acl = operation == "Deposit" ? bankAccount.DepositAcl : bankAccount.WithdrawAcl;
                return acl == "Everyone"; // Simplified for now
            }

            return false;
        }

        public async Task<bool> IsWalletActiveAsync(Guid walletId, string walletType)
        {
            if (walletType == "Treasury")
            {
                var treasury = await _treasuryRepo.GetByID(walletId);
                return treasury?.Status == TreasuryStatus.Active;
            }

            if (walletType == "BankAccount")
            {
                var bankAccount = await _bankAccountRepo.GetByID(walletId);
                return bankAccount?.Status == BankAccountStatus.Active;
            }

            return false;
        }

        public async Task<bool> IsCategoryValidAsync(Guid categoryId)
        {
            var account = await _glAccountRepo.GetByID(categoryId);
            return account != null && account.Type == AccountType.Revenue && account.IsLeaf;
        }

        public async Task<bool> IsDateInOpenPeriodAsync(DateTime date)
        {
            // Simplified validation - can be extended with proper fiscal year logic
            return date <= DateTime.Now && date >= DateTime.Now.AddYears(-1);
        }

        public async Task<string> GenerateVoucherCodeAsync(VoucherType type, DateTime date)
        {
            var year = date.Year;
            var prefix = type == VoucherType.Income ? "INC" : "EXP";

            var lastVoucher = await _voucherRepo
                .Get(v => v.Type == type && v.Date.Year == year)
                .OrderByDescending(v => v.Code)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastVoucher != null && !string.IsNullOrEmpty(lastVoucher.Code))
            {
                var codeParts = lastVoucher.Code.Split('-');
                if (codeParts.Length == 3 && int.TryParse(codeParts[2], out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}-{year}-{nextNumber:D4}";
        }
        public async Task<Guid> GetWalletControlAccountIdAsync(Guid walletId, string walletType)
        {
            if (walletType == "BankAccount")
            {
                var bank = await _bankAccountRepo.GetByID(walletId);
                return bank?.JournalAccountId ?? throw new InvalidOperationException("Bank account does not have a GL account assigned");
            }
            else if (walletType == "Treasury")
            {
                var treasury = await _treasuryRepo.GetByID(walletId);
                return treasury?.JournalAccountId ?? throw new InvalidOperationException("Treasury does not have a GL account assigned");
            }
            else
            {
                throw new InvalidOperationException("Unknown wallet type");
            }
        }
    }
}

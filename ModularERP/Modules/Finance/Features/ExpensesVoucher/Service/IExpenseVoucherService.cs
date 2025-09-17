using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Service
{
    public interface IExpenseVoucherService
    {
        Task<bool> ValidateWalletPermissionAsync(Guid walletId, string walletType, Guid userId, string operation);
        Task<bool> IsWalletActiveAsync(Guid walletId, string walletType);
        Task<bool> IsCategoryValidAsync(Guid categoryId);
        Task<bool> IsDateInOpenPeriodAsync(DateTime date);
        Task<string> GenerateVoucherCodeAsync(VoucherType type, DateTime date);
        Task<Guid> GetWalletControlAccountIdAsync(Guid walletId, string walletType);

    }
}

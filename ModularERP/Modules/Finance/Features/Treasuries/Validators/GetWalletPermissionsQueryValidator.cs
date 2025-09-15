using FluentValidation;
using ModularERP.Modules.Finance.Features.Treasuries.Queries;

namespace ModularERP.Modules.Finance.Features.Treasuries.Validators
{
    public class GetWalletPermissionsQueryValidator : AbstractValidator<GetWalletPermissionsQuery>
    {
        public GetWalletPermissionsQueryValidator()
        {
            RuleFor(x => x.WalletId).NotEmpty().WithMessage("Wallet ID is required");
            RuleFor(x => x.WalletType).NotEmpty().Must(x => x.ToLower() == "treasury" || x.ToLower() == "bankaccount")
                .WithMessage("Wallet type must be 'Treasury' or 'BankAccount'");
        }
    }
}

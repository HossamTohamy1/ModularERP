using FluentValidation;
using ModularERP.Modules.Finance.Features.WalletPermissions.Queries;

namespace ModularERP.Modules.Finance.Features.WalletPermissions.Validators
{
    public class GetAllWalletsQueryValidator : AbstractValidator<GetAllWalletsQuery>
    {
        public GetAllWalletsQueryValidator()
        {
            RuleFor(x => x.WalletType).Must(x => string.IsNullOrEmpty(x) || x.ToLower() == "treasury" || x.ToLower() == "bankaccount")
                .WithMessage("Wallet type must be 'Treasury' or 'BankAccount' if provided");
        }
    }
}

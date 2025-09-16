using FluentValidation;
using ModularERP.Modules.Finance.Features.WalletPermissions.Queries;

namespace ModularERP.Modules.Finance.Features.WalletPermissions.Validators
{
    public class GetUserWalletsQueryValidator : AbstractValidator<GetUserWalletsQuery>
    {
        public GetUserWalletsQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");
            RuleFor(x => x.WalletType).Must(x => string.IsNullOrEmpty(x) || x.ToLower() == "treasury" || x.ToLower() == "bankaccount")
                .WithMessage("Wallet type must be 'Treasury' or 'BankAccount' if provided");
        }
    }
}

using FluentValidation;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;

namespace ModularERP.Modules.Finance.Features.Treasuries.Validators
{
    public class DeleteWalletPermissionCommandValidator : AbstractValidator<DeleteWalletPermissionCommand>
    {
        public DeleteWalletPermissionCommandValidator()
        {
            RuleFor(x => x.WalletId).NotEmpty().WithMessage("Wallet ID is required");
            RuleFor(x => x.WalletType).NotEmpty().Must(x => x.ToLower() == "treasury" || x.ToLower() == "bankaccount")
                .WithMessage("Wallet type must be 'Treasury' or 'BankAccount'");
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");
        }
    }
}

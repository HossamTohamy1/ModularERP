using FluentValidation;
using ModularERP.Modules.Finance.Features.BankAccounts.Queries;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Validators
{
    public class GetBankAccountByIdQueryValidator : AbstractValidator<GetBankAccountByIdQuery>
    {
        public GetBankAccountByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Bank account ID is required");
        }
    }
}

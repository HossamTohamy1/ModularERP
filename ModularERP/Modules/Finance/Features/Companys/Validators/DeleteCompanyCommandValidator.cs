using FluentValidation;
using ModularERP.Modules.Finance.Features.Companys.Commands;

namespace ModularERP.Modules.Finance.Features.Companys.Validators
{
    public class DeleteCompanyCommandValidator : AbstractValidator<DeleteCompanyCommand>
    {
        public DeleteCompanyCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Company ID is required");
        }
    }
}

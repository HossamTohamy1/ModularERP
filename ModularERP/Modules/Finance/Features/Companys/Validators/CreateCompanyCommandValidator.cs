using FluentValidation;
using ModularERP.Modules.Finance.Features.Companys.Commands;
using ModularERP.Modules.Finance.Features.Companys.DTO;

namespace ModularERP.Modules.Finance.Features.Companys.Validators
{
    public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
    {
        public CreateCompanyCommandValidator(IValidator<CreateCompanyDto> dtoValidator)
        {
            RuleFor(x => x.CompanyDto).NotEmpty()
                .NotNull().WithMessage("Company data is required")
                .SetValidator(dtoValidator);
        }
    }
}

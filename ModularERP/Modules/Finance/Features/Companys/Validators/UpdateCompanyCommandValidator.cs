using FluentValidation;
using ModularERP.Modules.Finance.Features.Companys.Commands;
using ModularERP.Modules.Finance.Features.Companys.DTO;

namespace ModularERP.Modules.Finance.Features.Companys.Validators
{
    public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
    {
        public UpdateCompanyCommandValidator(IValidator<UpdateCompanyDto> dtoValidator)
        {
            RuleFor(x => x.CompanyDto)
                .NotNull().WithMessage("Company data is required")
                .SetValidator(dtoValidator);
        }
    }
}

using FluentValidation;
using ModularERP.Modules.Finance.Features.Taxs.Queries;

namespace ModularERP.Modules.Finance.Features.Taxs.Validators
{
    public class GetTaxByIdQueryValidator : AbstractValidator<GetTaxByIdQuery>
    {
        public GetTaxByIdQueryValidator()
        {
            RuleFor(x => x.TaxId)
                .NotEmpty()
                .WithMessage("Tax ID is required");
        }
    }
}

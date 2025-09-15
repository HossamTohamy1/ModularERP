using FluentValidation;
using ModularERP.Modules.Finance.Features.Treasuries.Queries;

namespace ModularERP.Modules.Finance.Features.Treasuries.Validators
{
    public class GetTreasuryByIdQueryValidator : AbstractValidator<GetTreasuryByIdQuery>
    {
        public GetTreasuryByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Treasury ID is required");
        }
    }
}

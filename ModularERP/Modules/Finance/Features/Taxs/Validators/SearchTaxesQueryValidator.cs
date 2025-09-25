using FluentValidation;
using ModularERP.Modules.Finance.Features.Taxs.Queries;

namespace ModularERP.Modules.Finance.Features.Taxs.Validators
{
    public class SearchTaxesQueryValidator : AbstractValidator<SearchTaxesQuery>
    {
        public SearchTaxesQueryValidator()
        {
            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .WithMessage("Search term cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));
        }
    }
}

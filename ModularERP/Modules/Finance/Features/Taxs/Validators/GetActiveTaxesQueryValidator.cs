using FluentValidation;
using ModularERP.Modules.Finance.Features.Taxs.Queries;

namespace ModularERP.Modules.Finance.Features.Taxs.Validators
{
    public class GetActiveTaxesQueryValidator : AbstractValidator<GetActiveTaxesQuery>
    {
        public GetActiveTaxesQueryValidator()
        {
            // No specific validation rules needed for GetActiveTaxesQuery
            // This validator exists for consistency and future extensibility
        }
    }
}

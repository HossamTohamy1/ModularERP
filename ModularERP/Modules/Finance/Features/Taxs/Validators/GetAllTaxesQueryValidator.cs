using FluentValidation;
using ModularERP.Modules.Finance.Features.Taxs.Queries;

namespace ModularERP.Modules.Finance.Features.Taxs.Validators
{
    public class GetAllTaxesQueryValidator : AbstractValidator<GetAllTaxesQuery>
    {
        public GetAllTaxesQueryValidator()
        {
            // No specific validation rules needed for GetAllTaxesQuery
            // This validator exists for consistency and future extensibility
        }
    }
}

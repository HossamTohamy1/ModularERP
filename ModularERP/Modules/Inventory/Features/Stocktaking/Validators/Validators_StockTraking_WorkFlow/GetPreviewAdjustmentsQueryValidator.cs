using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTraking_WorkFlow;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTraking_WorkFlow
{
    public class GetPreviewAdjustmentsQueryValidator : AbstractValidator<GetPreviewAdjustmentsQuery>
    {
        public GetPreviewAdjustmentsQueryValidator()
        {
            RuleFor(x => x.StocktakingId)
                .NotEmpty()
                .WithMessage("Stocktaking ID is required");

            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");
        }
    }
}

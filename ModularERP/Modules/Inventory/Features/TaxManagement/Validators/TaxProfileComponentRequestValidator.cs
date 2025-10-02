using FluentValidation;
using ModularERP.Modules.Inventory.Features.TaxManagement.DTO;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Validators
{
    public class TaxProfileComponentRequestValidator : AbstractValidator<TaxProfileComponentRequest>
    {
        public TaxProfileComponentRequestValidator()
        {
            RuleFor(x => x.TaxComponentId)
                .NotEmpty().WithMessage("Tax Component Id is required");

            RuleFor(x => x.Priority)
                .InclusiveBetween(1, 100).WithMessage("Priority must be between 1 and 100");
        }
    }
}
using FluentValidation;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Validators
{
    public class UpdateTaxComponentValidator : AbstractValidator<UpdateTaxComponentCommand>
    {
        public UpdateTaxComponentValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.RateType)
                .IsInEnum().WithMessage("Invalid Rate Type");

            RuleFor(x => x.RateValue)
                .InclusiveBetween(0, 100).WithMessage("Rate Value must be between 0 and 100");

            RuleFor(x => x.IncludedType)
                .IsInEnum().WithMessage("Invalid Included Type");

            RuleFor(x => x.AppliesOn)
                .IsInEnum().WithMessage("Invalid Applies On value");
        }
    }
}

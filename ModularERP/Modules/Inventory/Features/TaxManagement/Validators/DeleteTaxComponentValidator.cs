using FluentValidation;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Validators
{
    public class DeleteTaxComponentValidator : AbstractValidator<DeleteTaxComponentCommand>
    {
        public DeleteTaxComponentValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required");
        }
    }
}

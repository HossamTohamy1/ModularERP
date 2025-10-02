using FluentValidation;
using ModularERP.Modules.Inventory.Features.TaxManagement.Commends;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Validators
{
    public class DeleteTaxProfileValidator : AbstractValidator<DeleteTaxProfileCommand>
    {
        public DeleteTaxProfileValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required");
        }
    }
}

using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Command_Custom;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_CustomField
{
    public class DeleteCustomFieldValidator : AbstractValidator<DeleteCustomFieldCommand>
    {
        public DeleteCustomFieldValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Custom field ID is required");
        }
    }
}
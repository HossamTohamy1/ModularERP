using FluentValidation;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commands_ItemGroup;

namespace ModularERP.Modules.Inventory.Features.Products.Validators.Validators_ItemGroup
{
    public class CreateItemGroupCommandValidator : AbstractValidator<CreateItemGroupCommand>
    {
        public CreateItemGroupCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters");
        }
    }
}

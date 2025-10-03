using FluentValidation;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commands_ItemGroup;

namespace ModularERP.Modules.Inventory.Features.Products.Validators.Validators_ItemGroup
{
    public class UpdateItemGroupCommandValidator : AbstractValidator<UpdateItemGroupCommand>
    {
        public UpdateItemGroupCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters");
        }
    }
}

using FluentValidation;
using ModularERP.Modules.Inventory.Features.Products.Commends.Commands_ItemGroup;

namespace ModularERP.Modules.Inventory.Features.Products.Validators.Validators_ItemGroup
{
    public class AddItemToGroupCommandValidator : AbstractValidator<AddItemToGroupCommand>
    {
        public AddItemToGroupCommandValidator()
        {
            RuleFor(x => x.GroupId)
                .NotEmpty().WithMessage("GroupId is required");

            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("ProductId is required");

            RuleFor(x => x.SKU)
                .MaximumLength(100).WithMessage("SKU must not exceed 100 characters");

            RuleFor(x => x.Barcode)
                .MaximumLength(100).WithMessage("Barcode must not exceed 100 characters");

            RuleFor(x => x.PurchasePrice)
                .GreaterThanOrEqualTo(0).When(x => x.PurchasePrice.HasValue)
                .WithMessage("Purchase price must be greater than or equal to 0");

            RuleFor(x => x.SellingPrice)
                .GreaterThanOrEqualTo(0).When(x => x.SellingPrice.HasValue)
                .WithMessage("Selling price must be greater than or equal to 0");
        }
    }
}

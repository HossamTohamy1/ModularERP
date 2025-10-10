using FluentValidation;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_BulkDiscount;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_BulkDiscount
{
    public class UpdateBulkDiscountCommandValidator : AbstractValidator<UpdateBulkDiscountCommand>
    {
        public UpdateBulkDiscountCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Bulk Discount ID is required");

            RuleFor(x => x.PriceListId)
                .NotEmpty()
                .WithMessage("Price List ID is required");

            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            RuleFor(x => x.MinQty)
                .GreaterThan(0)
                .WithMessage("Minimum quantity must be greater than 0")
                .PrecisionScale(18, 3, true)
                .WithMessage("Minimum quantity must have maximum 3 decimal places");

            RuleFor(x => x.MaxQty)
                .GreaterThan(x => x.MinQty)
                .When(x => x.MaxQty.HasValue)
                .WithMessage("Maximum quantity must be greater than minimum quantity")
                .PrecisionScale(18, 3, true)
                .When(x => x.MaxQty.HasValue)
                .WithMessage("Maximum quantity must have maximum 3 decimal places");

            RuleFor(x => x.DiscountType)
                .IsInEnum()
                .WithMessage("Invalid discount type");

            RuleFor(x => x.DiscountValue)
                .GreaterThan(0)
                .WithMessage("Discount value must be greater than 0")
                .PrecisionScale(18, 4, true)
                .WithMessage("Discount value must have maximum 4 decimal places");

            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100)
                .When(x => x.DiscountType == DiscountType.Percentage)
                .WithMessage("Percentage discount cannot exceed 100%");
        }
    }

}

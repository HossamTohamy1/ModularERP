using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_BulkDiscount;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_BulkDiscount
{
    public class DeleteBulkDiscountCommandValidator : AbstractValidator<DeleteBulkDiscountCommand>
    {
        public DeleteBulkDiscountCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Bulk Discount ID is required");

            RuleFor(x => x.PriceListId)
                .NotEmpty()
                .WithMessage("Price List ID is required");
        }
    }
}
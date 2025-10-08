using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceList
{
    public class BulkUpdatePriceListItemValidator : AbstractValidator<BulkUpdatePriceListItemDto>
    {
        public BulkUpdatePriceListItemValidator()
        {
            RuleFor(x => x.Items)
                .NotNull()
                .NotEmpty()
                .WithMessage("Items list cannot be empty");

            RuleForEach(x => x.Items)
                .ChildRules(item =>
                {
                    item.RuleFor(x => x.ItemId)
                        .NotEmpty()
                        .WithMessage("ItemId is required");

                    item.RuleFor(x => x.BasePrice)
                        .GreaterThanOrEqualTo(0)
                        .When(x => x.BasePrice.HasValue)
                        .WithMessage("Base price must be greater than or equal to 0");

                    item.RuleFor(x => x.ListPrice)
                        .GreaterThanOrEqualTo(0)
                        .When(x => x.ListPrice.HasValue)
                        .WithMessage("List price must be greater than or equal to 0");

                    item.RuleFor(x => x.DiscountValue)
                        .GreaterThanOrEqualTo(0)
                        .When(x => x.DiscountValue.HasValue)
                        .WithMessage("Discount value must be greater than or equal to 0");
                });
        }
    }
}
using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commands_PriceCalculation;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceCalculation
{
    public class CalculatePriceCommandValidator : AbstractValidator<CalculatePriceCommand>
    {
        public CalculatePriceCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero");

            RuleFor(x => x.TransactionType)
                .NotEmpty()
                .WithMessage("Transaction type is required")
                .Must(type => new[] { "Sale", "Purchase", "Quote" }.Contains(type))
                .WithMessage("Transaction type must be Sale, Purchase, or Quote");
        }
    }
}

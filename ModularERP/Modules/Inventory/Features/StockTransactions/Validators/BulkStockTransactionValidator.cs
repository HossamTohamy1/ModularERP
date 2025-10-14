using FluentValidation;
using ModularERP.Modules.Inventory.Features.StockTransactions.Validators;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Validators
{
    public class BulkStockTransactionValidator : AbstractValidator<BulkStockTransactionDto>
    {
        public BulkStockTransactionValidator()
        {
            RuleFor(x => x.Transactions)
                .NotEmpty()
                .WithMessage("At least one transaction is required")
                .Must(x => x.Count <= 1000)
                .WithMessage("Cannot process more than 1000 transactions at once");

            RuleForEach(x => x.Transactions)
                .SetValidator(new CreateStockTransactionValidator());
        }
    }
}

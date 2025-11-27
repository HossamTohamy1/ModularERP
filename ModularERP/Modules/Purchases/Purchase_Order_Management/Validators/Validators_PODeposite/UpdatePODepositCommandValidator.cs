using FluentValidation;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PODeposite;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Validators.Validators_PODeposite
{
    public class UpdatePODepositCommandValidator : AbstractValidator<UpdatePODepositCommand>
    {
        public UpdatePODepositCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Deposit ID is required");

            RuleFor(x => x.PurchaseOrderId)
                .NotEmpty().WithMessage("Purchase Order ID is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero");

            RuleFor(x => x.Percentage)
                .InclusiveBetween(0, 100).WithMessage("Percentage must be between 0 and 100")
                .When(x => x.Percentage.HasValue);


            RuleFor(x => x.ReferenceNumber)
                .MaximumLength(100).WithMessage("Reference number must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

            RuleFor(x => x.PaymentDate)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Payment date cannot be in the future")
                .When(x => x.PaymentDate.HasValue);

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}

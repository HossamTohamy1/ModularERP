using FluentValidation;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;

namespace ModularERP.Modules.Purchases.Refunds.Validators.Validators__RefundItem
{
    public class CreateRefundFromPOCommandValidator : AbstractValidator<CreateRefundFromPOCommand>
    {
        public CreateRefundFromPOCommandValidator()
        {
            RuleFor(x => x.PurchaseOrderId)
                .NotEmpty()
                .WithMessage("Purchase Order ID is required");

            RuleFor(x => x.LineItems)
                .NotEmpty()
                .WithMessage("At least one line item is required");

            RuleForEach(x => x.LineItems)
                .SetValidator(new CreateRefundLineItemDtoValidator());

            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .WithMessage("Reason cannot exceed 500 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Notes cannot exceed 1000 characters");
        }
    }
}

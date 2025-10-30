using FluentValidation;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commenads_POStauts;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Validators.Validators_POStatus
{
    public class UpdatePONotesValidator : AbstractValidator<UpdatePONotesCommand>
    {
        public UpdatePONotesValidator()
        {
            RuleFor(x => x.PurchaseOrderId)
                .NotEmpty()
                .WithMessage("Purchase Order ID is required");

            RuleFor(x => x.Notes)
                .MaximumLength(2000)
                .WithMessage("Notes cannot exceed 2000 characters");

            RuleFor(x => x.Terms)
                .MaximumLength(2000)
                .WithMessage("Terms cannot exceed 2000 characters");

            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Notes) || !string.IsNullOrWhiteSpace(x.Terms))
                .WithMessage("At least one field (Notes or Terms) must be provided");
        }
    }
}

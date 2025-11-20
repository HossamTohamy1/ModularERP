using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_InvocieItem
{
    public class AddInvoiceItemValidator : AbstractValidator<AddInvoiceItemRequest>
    {
        public AddInvoiceItemValidator()
        {
            RuleFor(x => x.POLineItemId)
                .NotEmpty().WithMessage("PO Line Item ID is required");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0");

            RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Tax amount must be greater than or equal to 0");
        }
    }
}

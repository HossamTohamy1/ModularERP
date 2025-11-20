using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_Invoice
{
    public class UpdateInvoiceLineItemDtoValidator : AbstractValidator<UpdateInvoiceLineItemDto>
    {
        public UpdateInvoiceLineItemDtoValidator()
        {
            RuleFor(x => x.POLineItemId)
                .NotEmpty().WithMessage("PO Line Item ID is required");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative");

            RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Tax amount cannot be negative");

            RuleFor(x => x.LineTotal)
                .GreaterThanOrEqualTo(0).WithMessage("Line total cannot be negative");
        }
    }
}

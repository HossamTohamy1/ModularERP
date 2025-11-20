using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_Invoice;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_Invoice
{
    public class CreatePurchaseInvoiceCommandValidator : AbstractValidator<CreatePurchaseInvoiceCommand>
    {
        public CreatePurchaseInvoiceCommandValidator()
        {
            RuleFor(x => x.PurchaseOrderId)
                .NotEmpty().WithMessage("Purchase Order ID is required");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("Company ID is required");

            RuleFor(x => x.SupplierId)
                .NotEmpty().WithMessage("Supplier ID is required");

            RuleFor(x => x.InvoiceDate)
                .NotEmpty().WithMessage("Invoice date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Invoice date cannot be in the future");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(x => x.InvoiceDate)
                .When(x => x.DueDate.HasValue)
                .WithMessage("Due date must be after invoice date");

            RuleFor(x => x.DepositApplied)
                .GreaterThanOrEqualTo(0).WithMessage("Deposit amount cannot be negative");

            RuleFor(x => x.LineItems)
                .NotEmpty().WithMessage("At least one line item is required")
                .Must(items => items != null && items.Count > 0)
                .WithMessage("Invoice must have at least one line item");

            RuleForEach(x => x.LineItems)
                .SetValidator(new CreateInvoiceLineItemDtoValidator());
        }
    }

}

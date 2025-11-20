using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_Invoice;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_Invoice
{
    public class CreateInvoiceLineItemDtoValidator : AbstractValidator<CreateInvoiceLineItemDto>
    {
        public CreateInvoiceLineItemDtoValidator()
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

    public class UpdatePurchaseInvoiceCommandValidator : AbstractValidator<UpdatePurchaseInvoiceCommand>
    {
        public UpdatePurchaseInvoiceCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Invoice ID is required");

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
                .NotEmpty().WithMessage("At least one line item is required");

            RuleForEach(x => x.LineItems)
                .SetValidator(new UpdateInvoiceLineItemDtoValidator());
        }
    }

}

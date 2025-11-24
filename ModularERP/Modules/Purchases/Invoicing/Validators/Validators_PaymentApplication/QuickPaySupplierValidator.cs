using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_PaymentApplication
{
    public class QuickPaySupplierValidator : AbstractValidator<QuickPaySupplierDto>
    {
        public QuickPaySupplierValidator()
        {
            RuleFor(x => x.SupplierId)
                .NotEmpty()
                .WithMessage("Supplier ID is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Payment amount must be greater than zero");

            RuleFor(x => x.PaymentType)
                .NotEmpty()
                .WithMessage("Payment type is required")
                .Must(x => new[] { "AgainstInvoice", "Advance", "Refund" }.Contains(x))
                .WithMessage("Payment type must be one of: AgainstInvoice, Advance, Refund");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty()
                .WithMessage("Payment method is required")
                .Must(x => new[] { "Cash", "Bank", "Cheque", "Card" }.Contains(x))
                .WithMessage("Payment method must be one of: Cash, Bank, Cheque, Card");

            RuleFor(x => x.PaymentDate)
                .NotEmpty()
                .WithMessage("Payment date is required")
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
                .WithMessage("Payment date cannot be in the future");

            When(x => x.PaymentType == "AgainstInvoice", () =>
            {
                RuleFor(x => x.InvoiceAllocations)
                    .NotEmpty()
                    .WithMessage("Invoice allocations are required when payment type is 'AgainstInvoice'");

                RuleForEach(x => x.InvoiceAllocations)
                    .SetValidator(new InvoiceAllocationValidator());
            });
        }
    }
}

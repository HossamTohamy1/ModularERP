using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_SupplierPayments
{
    public class CreateSupplierPaymentValidator : AbstractValidator<CreateSupplierPaymentDto>
    {
        public CreateSupplierPaymentValidator()
        {
            // Supplier validation
            RuleFor(x => x.SupplierId)
                .NotEmpty().WithMessage("Supplier is required");

            // Payment Type validation - updated to match BRSD
            RuleFor(x => x.PaymentType)
                .NotEmpty().WithMessage("Payment type is required")
                .Must(x => new[] { "AgainstInvoice", "Deposit", "Advance" }.Contains(x))
                .WithMessage("Invalid payment type. Allowed values: AgainstInvoice, Deposit, Advance");

            // Payment Method validation
            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required")
                .MaximumLength(50).WithMessage("Payment method must not exceed 50 characters");

            // Payment Date validation
            RuleFor(x => x.PaymentDate)
                .NotEmpty().WithMessage("Payment date is required")
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Payment date cannot be in the future");

            // Amount validation
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero");

            // Allocated Amount validation
            RuleFor(x => x.AllocatedAmount)
                .LessThanOrEqualTo(x => x.Amount)
                .When(x => x.AllocatedAmount.HasValue)
                .WithMessage("Allocated amount cannot exceed total amount");

            RuleFor(x => x.AllocatedAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.AllocatedAmount.HasValue)
                .WithMessage("Allocated amount cannot be negative");

            // Invoice validation - required for AgainstInvoice type
            RuleFor(x => x.InvoiceId)
                .NotEmpty()
                .When(x => x.PaymentType == "AgainstInvoice")
                .WithMessage("Invoice is required when payment type is 'AgainstInvoice'");

            // Invoice should NOT be provided for Deposit/Advance
            RuleFor(x => x.InvoiceId)
                .Must(x => !x.HasValue)
                .When(x => x.PaymentType == "Deposit" || x.PaymentType == "Advance")
                .WithMessage("Invoice should not be provided for Deposit or Advance payment types");

            // AllocatedAmount should NOT be provided for Deposit/Advance
            RuleFor(x => x.AllocatedAmount)
                .Must(x => !x.HasValue || x == 0)
                .When(x => x.PaymentType == "Deposit" || x.PaymentType == "Advance")
                .WithMessage("Allocated amount should not be provided for Deposit or Advance payments (they are unallocated by default)");


            // PO validation for Deposit/Advance (optional but recommended)
            RuleFor(x => x.PurchaseOrderId)
                .NotEmpty()
                .When(x => x.PaymentType == "Deposit" || x.PaymentType == "Advance")
                .WithMessage("Purchase Order is recommended for Deposit or Advance payments")
                .WithSeverity(Severity.Warning);

            // Reference Number validation
            RuleFor(x => x.ReferenceNumber)
                .MaximumLength(100).WithMessage("Reference number must not exceed 100 characters");

            // Notes validation
            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");
        }
    }
}
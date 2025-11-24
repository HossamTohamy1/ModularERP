using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_PaymentApplication
{
    public class ApplyPaymentValidator : AbstractValidator<ApplyPaymentDto>
    {
        public ApplyPaymentValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty()
                .WithMessage("Payment ID is required");

            RuleFor(x => x.Allocations)
                .NotEmpty()
                .WithMessage("At least one allocation is required");

            RuleForEach(x => x.Allocations)
                .SetValidator(new InvoiceAllocationValidator());
        }
    }

    public class InvoiceAllocationValidator : AbstractValidator<InvoiceAllocationDto>
    {
        public InvoiceAllocationValidator()
        {
            RuleFor(x => x.InvoiceId)
                .NotEmpty()
                .WithMessage("Invoice ID is required");

            RuleFor(x => x.AllocatedAmount)
                .GreaterThan(0)
                .WithMessage("Allocated amount must be greater than zero");
        }
    }
}

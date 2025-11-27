using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_SupplierPayments
{
    public class UpdateSupplierPaymentValidator : AbstractValidator<UpdateSupplierPaymentDto>
    {
        public UpdateSupplierPaymentValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Payment ID is required");



            RuleFor(x => x.PaymentDate)
                .NotEmpty().WithMessage("Payment date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Payment date cannot be in the future");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero");

            RuleFor(x => x.ReferenceNumber)
                .MaximumLength(100).WithMessage("Reference number must not exceed 100 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");
        }
    }
}

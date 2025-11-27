using FluentValidation;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentTerm;

namespace ModularERP.Modules.Purchases.Payment.Validators.Validators_PaymentTerm
{
    public class CreatePaymentTermValidator : AbstractValidator<CreatePaymentTermDto>
    {
        public CreatePaymentTermValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Days)
                .InclusiveBetween(0, 365).WithMessage("Days must be between 0 and 365");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}

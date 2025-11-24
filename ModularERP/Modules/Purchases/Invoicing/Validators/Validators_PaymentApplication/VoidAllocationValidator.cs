using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_PaymentApplication
{
    public class VoidAllocationValidator : AbstractValidator<VoidAllocationDto>
    {
        public VoidAllocationValidator()
        {
            RuleFor(x => x.AllocationId)
                .NotEmpty()
                .WithMessage("Allocation ID is required");

            RuleFor(x => x.VoidReason)
                .NotEmpty()
                .WithMessage("Void reason is required")
                .MaximumLength(500)
                .WithMessage("Void reason cannot exceed 500 characters");
        }
    }
}

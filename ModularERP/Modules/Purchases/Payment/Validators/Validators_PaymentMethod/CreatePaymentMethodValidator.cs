using FluentValidation;
using ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentMethod;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Payment.Validators.Validators_PaymentMethod
{
    public class CreatePaymentMethodValidator : AbstractValidator<CreatePaymentMethodCommand>
    {
        private readonly IGeneralRepository<PaymentMethod> _repository;

        public CreatePaymentMethodValidator(IGeneralRepository<PaymentMethod> repository)
        {
            _repository = repository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Payment method name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
                .MustAsync(BeUniqueName).WithMessage("Payment method name already exists");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Payment method code is required")
                .MaximumLength(50).WithMessage("Code cannot exceed 50 characters")
                .Matches("^[A-Z0-9_]+$").WithMessage("Code must contain only uppercase letters, numbers, and underscores")
                .MustAsync(BeUniqueCode).WithMessage("Payment method code already exists");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }

        private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
        {
            return !await _repository.AnyAsync(x => x.Name.ToLower() == name.ToLower(), cancellationToken);
        }

        private async Task<bool> BeUniqueCode(string code, CancellationToken cancellationToken)
        {
            return !await _repository.AnyAsync(x => x.Code.ToUpper() == code.ToUpper(), cancellationToken);
        }
    }
}

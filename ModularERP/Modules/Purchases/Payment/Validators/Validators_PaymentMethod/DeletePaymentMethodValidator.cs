using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ModularERP.Modules.Purchases.Payment.Commends.Commends_PaymentMethod;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Payment.Validators.Validators_PaymentMethod
{
    public class DeletePaymentMethodValidator : AbstractValidator<DeletePaymentMethodCommand>
    {
        private readonly IGeneralRepository<PaymentMethod> _repository;

        public DeletePaymentMethodValidator(IGeneralRepository<PaymentMethod> repository)
        {
            _repository = repository;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Payment method ID is required")
                .MustAsync(NotBeUsedInTransactions)
                .WithMessage("Cannot delete payment method that is used in transactions. Consider deactivating it instead.");
        }

        private async Task<bool> NotBeUsedInTransactions(Guid id, CancellationToken cancellationToken)
        {
            var paymentMethod = await _repository
                .GetAll()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    HasPayments = x.SupplierPayments.Any(),
                    HasDeposits = x.PODeposits.Any()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (paymentMethod == null)
                return false;

            return !paymentMethod.HasPayments && !paymentMethod.HasDeposits;
        }
    }
}
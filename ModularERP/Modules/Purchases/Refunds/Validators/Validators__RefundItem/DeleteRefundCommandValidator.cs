using FluentValidation;
using ModularERP.Modules.Purchases.Refunds.Commends.Commends_RefundInvoce;

namespace ModularERP.Modules.Purchases.Refunds.Validators.Validators__RefundItem
{
    public class DeleteRefundCommandValidator : AbstractValidator<DeleteRefundCommand>
    {
        public DeleteRefundCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Refund ID is required");
        }
    }
}

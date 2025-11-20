using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_InvociePayment;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_InvoicePayment
{
    public class GetInvoiceBalanceQueryValidator : AbstractValidator<GetInvoiceBalanceQuery>
    {
        public GetInvoiceBalanceQueryValidator()
        {
            RuleFor(x => x.InvoiceId)
                .NotEmpty()
                .WithMessage("Invoice ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("Invoice ID cannot be empty GUID");
        }
    }
}
using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_InvociePayment;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_InvoicePayment
{
    public class GetInvoicePaymentsQueryValidator : AbstractValidator<GetInvoicePaymentsQuery>
    {
        public GetInvoicePaymentsQueryValidator()
        {
            RuleFor(x => x.InvoiceId)
                .NotEmpty()
                .WithMessage("Invoice ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("Invoice ID cannot be empty GUID");
        }
    }

}

using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_Invoice;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_Invoice
{
    public class DeletePurchaseInvoiceCommandValidator : AbstractValidator<DeletePurchaseInvoiceCommand>
    {
        public DeletePurchaseInvoiceCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Invoice ID is required");
        }
    }
}
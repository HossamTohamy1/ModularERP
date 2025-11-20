using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_InvocieItem
{
    public class CreateInvoiceFromPOValidator : AbstractValidator<CreateInvoiceFromPORequest>
    {
        public CreateInvoiceFromPOValidator()
        {
            RuleFor(x => x.InvoiceDate)
                .NotEmpty().WithMessage("Invoice date is required");

            RuleFor(x => x.DepositApplied)
                .GreaterThanOrEqualTo(0).WithMessage("Deposit applied must be greater than or equal to 0");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(x => x.InvoiceDate)
                .When(x => x.DueDate.HasValue)
                .WithMessage("Due date must be greater than or equal to invoice date");
        }
    }
}

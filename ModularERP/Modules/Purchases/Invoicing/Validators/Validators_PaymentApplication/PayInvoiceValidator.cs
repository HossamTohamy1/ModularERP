using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication;

namespace ModularERP.Modules.Purchases.Invoicing.Validators.Validators_PaymentApplication
{
    public class PayInvoiceValidator : AbstractValidator<PayInvoiceDto>
    {
        public PayInvoiceValidator()
        {
            RuleFor(x => x.InvoiceId)
                .NotEmpty()
                .WithMessage("Invoice ID is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Payment amount must be greater than zero");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty()
                .WithMessage("Payment method is required")
                .Must(x => new[] { "Cash", "Bank", "Cheque", "Card" }.Contains(x))
                .WithMessage("Payment method must be one of: Cash, Bank, Cheque, Card");

            RuleFor(x => x.PaymentDate)
                .NotEmpty()
                .WithMessage("Payment date is required")
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
                .WithMessage("Payment date cannot be in the future");
        }
    }
}

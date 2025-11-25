using FluentValidation;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Queries_DebitNote;

namespace ModularERP.Modules.Purchases.Refunds.Validators.Validators_DebitNote
{
    public class GetDebitNotesBySupplierQueryValidator : AbstractValidator<GetDebitNotesBySupplierQuery>
    {
        public GetDebitNotesBySupplierQueryValidator()
        {
            RuleFor(x => x.SupplierId)
                .NotEmpty()
                .WithMessage("Supplier ID is required");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.FromDate)
                .LessThanOrEqualTo(x => x.ToDate)
                .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
                .WithMessage("From date must be before or equal to To date");
        }
    }
}

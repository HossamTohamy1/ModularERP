using FluentValidation;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Queries_DebitNote;

namespace ModularERP.Modules.Purchases.Refunds.Validators.Validators_DebitNote
{
    public class GetDebitNoteRefundQueryValidator : AbstractValidator<GetDebitNoteRefundQuery>
    {
        public GetDebitNoteRefundQueryValidator()
        {
            RuleFor(x => x.DebitNoteId)
                .NotEmpty()
                .WithMessage("Debit Note ID is required");
        }
    }
}

using FluentValidation;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Queries_DebitNote;

namespace ModularERP.Modules.Purchases.Refunds.Validators.Validators_DebitNote
{
    public class GetDebitNoteByIdQueryValidator : AbstractValidator<GetDebitNoteByIdQuery>
    {
        public GetDebitNoteByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Debit Note ID is required");
        }
    }

}

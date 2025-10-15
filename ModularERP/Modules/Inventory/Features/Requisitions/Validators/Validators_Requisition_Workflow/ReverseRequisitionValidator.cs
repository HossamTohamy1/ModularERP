using FluentValidation;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition_Workflow;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Validators.Validators_Requisition_Workflow
{
    public class ReverseRequisitionValidator : AbstractValidator<ReverseRequisitionCommand>
    {
        public ReverseRequisitionValidator()
        {
            RuleFor(x => x.RequisitionId)
                .NotEmpty()
                .WithMessage("Requisition ID is required");

            RuleFor(x => x.Comments)
                .NotEmpty()
                .WithMessage("Reversal reason is required")
                .MinimumLength(10)
                .WithMessage("Reversal reason must be at least 10 characters")
                .MaximumLength(500)
                .WithMessage("Reversal reason cannot exceed 500 characters");
        }
    }
}
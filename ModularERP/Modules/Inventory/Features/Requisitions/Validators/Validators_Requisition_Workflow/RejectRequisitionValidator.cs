using FluentValidation;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition_Workflow;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Validators.Validators_Requisition_Workflow
{
    public class RejectRequisitionValidator : AbstractValidator<RejectRequisitionCommand>
    {
        public RejectRequisitionValidator()
        {
            RuleFor(x => x.RequisitionId)
                .NotEmpty()
                .WithMessage("Requisition ID is required");

            RuleFor(x => x.Comments)
                .NotEmpty()
                .WithMessage("Rejection reason is required")
                .MinimumLength(10)
                .WithMessage("Rejection reason must be at least 10 characters")
                .MaximumLength(500)
                .WithMessage("Rejection reason cannot exceed 500 characters");
        }
    }
}

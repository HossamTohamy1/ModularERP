using FluentValidation;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition_Workflow;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Validators.Validators_Requisition_Workflow
{
    public class CancelRequisitionValidator : AbstractValidator<CancelRequisitionCommand>
    {
        public CancelRequisitionValidator()
        {
            RuleFor(x => x.RequisitionId)
                .NotEmpty()
                .WithMessage("Requisition ID is required");

            RuleFor(x => x.Comments)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Comments))
                .WithMessage("Comments cannot exceed 500 characters");
        }
    }

}

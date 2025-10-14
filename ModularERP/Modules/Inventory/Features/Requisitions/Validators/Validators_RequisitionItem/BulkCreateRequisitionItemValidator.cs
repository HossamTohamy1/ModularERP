using FluentValidation;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Validators.Validators_RequisitionItem
{
    public class BulkCreateRequisitionItemValidator : AbstractValidator<BulkCreateRequisitionItemDTO>
    {
        public BulkCreateRequisitionItemValidator()
        {
            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("At least one item is required");

            RuleForEach(x => x.Items)
                .SetValidator(new CreateRequisitionItemValidator());
        }
    }
}
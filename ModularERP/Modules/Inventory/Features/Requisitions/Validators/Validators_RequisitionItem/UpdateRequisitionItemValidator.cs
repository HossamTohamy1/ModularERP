using FluentValidation;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Validators.Validators_RequisitionItem
{
    public class UpdateRequisitionItemValidator : AbstractValidator<UpdateRequisitionItemDTO>
    {
        public UpdateRequisitionItemValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product is required");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.UnitPrice.HasValue)
                .WithMessage("Unit price must be greater than or equal to 0");


        }
    }
}

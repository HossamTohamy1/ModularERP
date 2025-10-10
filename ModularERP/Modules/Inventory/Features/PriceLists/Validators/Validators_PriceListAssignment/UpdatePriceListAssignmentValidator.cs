using FluentValidation;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListAssignment;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceListAssignment
{
    public class UpdatePriceListAssignmentValidator : AbstractValidator<UpdatePriceListAssignmentDto>
    {
        public UpdatePriceListAssignmentValidator()
        {
            RuleFor(x => x.EntityType)
                .IsInEnum()
                .WithMessage("EntityType must be a valid value");

            RuleFor(x => x.EntityId)
                .NotEmpty()
                .WithMessage("EntityId is required");

            RuleFor(x => x.PriceListId)
                .NotEmpty()
                .WithMessage("PriceListId is required");
        }
    }
}
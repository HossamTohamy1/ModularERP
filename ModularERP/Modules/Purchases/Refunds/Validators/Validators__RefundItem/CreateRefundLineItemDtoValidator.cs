using FluentValidation;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;

namespace ModularERP.Modules.Purchases.Refunds.Validators.Validators__RefundItem
{
    public class CreateRefundLineItemDtoValidator : AbstractValidator<CreateRefundLineItemDto>
    {
        public CreateRefundLineItemDtoValidator()
        {
            RuleFor(x => x.GRNLineItemId)
                .NotEmpty()
                .WithMessage("GRN Line Item ID is required");

            RuleFor(x => x.ReturnQuantity)
                .GreaterThan(0)
                .WithMessage("Return quantity must be greater than zero");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Unit price must be non-negative");
        }
    }
}

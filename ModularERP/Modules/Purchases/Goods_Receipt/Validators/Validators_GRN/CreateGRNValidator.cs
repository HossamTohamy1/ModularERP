using FluentValidation;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Validators.Validators_GRN
{
    public class CreateGRNValidator : AbstractValidator<CreateGRNDto>
    {
        public CreateGRNValidator()
        {
            RuleFor(x => x.PurchaseOrderId)
                .NotEmpty().WithMessage("Purchase Order ID is required");

            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Warehouse ID is required");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("Company ID is required");

            RuleFor(x => x.ReceiptDate)
                .NotEmpty().WithMessage("Receipt Date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Receipt Date cannot be in the future");

            RuleFor(x => x.ReceivedBy)
                .MaximumLength(200).WithMessage("Received By cannot exceed 200 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");

            RuleFor(x => x.LineItems)
                .NotEmpty().WithMessage("At least one line item is required")
                .Must(items => items != null && items.Any()).WithMessage("GRN must have at least one line item");

            RuleForEach(x => x.LineItems).SetValidator(new CreateGRNLineItemValidator());
        }
    }

}

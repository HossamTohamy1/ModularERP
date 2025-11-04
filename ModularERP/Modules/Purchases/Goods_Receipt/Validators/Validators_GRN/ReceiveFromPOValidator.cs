using FluentValidation;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNPO;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Validators.Validators_GRN
{
    public class ReceiveFromPOValidator : AbstractValidator<ReceiveFromPOCommand>
    {
        public ReceiveFromPOValidator()
        {
            RuleFor(x => x.PurchaseOrderId)
                .NotEmpty()
                .WithMessage("Purchase Order ID is required");

            RuleFor(x => x.WarehouseId)
                .NotEmpty()
                .WithMessage("Warehouse ID is required");

            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company ID is required");

            RuleFor(x => x.ReceiptDate)
                .NotEmpty()
                .WithMessage("Receipt Date is required")
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Receipt Date cannot be in the future");

            RuleFor(x => x.LineItems)
                .NotEmpty()
                .WithMessage("At least one line item is required");

            RuleForEach(x => x.LineItems).ChildRules(item =>
            {
                item.RuleFor(x => x.POLineItemId)
                    .NotEmpty()
                    .WithMessage("PO Line Item ID is required");

                item.RuleFor(x => x.ReceivedQuantity)
                    .GreaterThan(0)
                    .WithMessage("Received quantity must be greater than zero");
            });
        }
    }
}
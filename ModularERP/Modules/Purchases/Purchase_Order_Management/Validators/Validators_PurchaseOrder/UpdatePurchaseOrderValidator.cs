using FluentValidation;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Validators.Validators_PurchaseOrder
{
    public class UpdatePurchaseOrderValidator : AbstractValidator<UpdatePurchaseOrderCommand>
    {
        public UpdatePurchaseOrderValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Purchase Order ID is required");

            RuleFor(x => x.SupplierId)
                .NotEmpty().WithMessage("Supplier is required");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required")
                .MaximumLength(3).WithMessage("Currency code must be 3 characters");

            RuleFor(x => x.PODate)
                .NotEmpty().WithMessage("PO date is required")
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
                .WithMessage("PO date cannot be in the future");

            RuleFor(x => x.LineItems)
                .NotEmpty().WithMessage("At least one line item is required")
                .Must(items => items != null && items.Count > 0)
                .WithMessage("Purchase order must contain at least one line item");

            RuleForEach(x => x.LineItems).ChildRules(item =>
            {
                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be greater than 0");

                item.RuleFor(x => x.UnitPrice)
                    .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative");

                item.RuleFor(x => x.DiscountPercent)
                    .InclusiveBetween(0, 100).WithMessage("Discount percent must be between 0 and 100");

                item.RuleFor(x => x.Description)
                    .NotEmpty().WithMessage("Item description is required")
                    .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

                item.RuleFor(x => x)
                    .Must(x => x.ProductId.HasValue || x.ServiceId.HasValue)
                    .WithMessage("Either Product or Service must be selected");
            });

            RuleForEach(x => x.Deposits).ChildRules(deposit =>
            {
                deposit.RuleFor(x => x.Amount)
                    .GreaterThan(0).WithMessage("Deposit amount must be greater than 0");




                deposit.RuleFor(x => x.Percentage)
                    .InclusiveBetween(0, 100).When(x => x.Percentage.HasValue)
                    .WithMessage("Percentage must be between 0 and 100");
            });

            RuleForEach(x => x.Discounts).ChildRules(discount =>
            {
                discount.RuleFor(x => x.DiscountType)
                    .NotEmpty().WithMessage("Discount type is required")
                    .Must(x => new[] { "Percentage", "Fixed" }.Contains(x))
                    .WithMessage("Discount type must be either 'Percentage' or 'Fixed'");

                discount.RuleFor(x => x.DiscountValue)
                    .GreaterThan(0).WithMessage("Discount value must be greater than 0");

                discount.RuleFor(x => x.DiscountValue)
                    .LessThanOrEqualTo(100)
                    .When(x => x.DiscountType == "Percentage")
                    .WithMessage("Percentage discount cannot exceed 100%");
            });

            RuleForEach(x => x.ShippingCharges).ChildRules(shipping =>
            {
                shipping.RuleFor(x => x.ShippingFee)
                    .GreaterThanOrEqualTo(0).WithMessage("Shipping fee cannot be negative");
            });

            RuleForEach(x => x.NewAttachments).ChildRules(file =>
            {
                file.RuleFor(x => x.Length)
                    .LessThanOrEqualTo(5 * 1024 * 1024)
                    .WithMessage("File size cannot exceed 5MB");

                file.RuleFor(x => x.ContentType)
                    .Must(ct => new[] { "application/pdf", "image/jpeg", "image/png", "image/jpg",
                                       "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" }
                                .Contains(ct))
                    .WithMessage("Only PDF, Images (JPG, PNG), and Word documents are allowed");
            });

        }



    }
}

using FluentValidation;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POItme;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Validators.Validators_POItme
{
    public class UpdatePOLineItemValidator : AbstractValidator<UpdatePOLineItemDto>
    {
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IGeneralRepository<POLineItem> _lineItemRepository;

        public UpdatePOLineItemValidator(
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<POLineItem> lineItemRepository)
        {
            _poRepository = poRepository;
            _lineItemRepository = lineItemRepository;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Line Item ID is required")
                .MustAsync(LineItemExists)
                .WithMessage("Line Item does not exist");

            RuleFor(x => x.PurchaseOrderId)
                .NotEmpty().WithMessage("Purchase Order ID is required")
                .MustAsync(PurchaseOrderExists)
                .WithMessage("Purchase Order does not exist");

            RuleFor(x => x)
                .Must(x => x.ProductId.HasValue || x.ServiceId.HasValue)
                .WithMessage("Either Product or Service must be specified");

            RuleFor(x => x)
                .Must(x => !(x.ProductId.HasValue && x.ServiceId.HasValue))
                .WithMessage("Cannot specify both Product and Service");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit Price cannot be negative");

            RuleFor(x => x.DiscountPercent)
                .InclusiveBetween(0, 100).WithMessage("Discount Percent must be between 0 and 100");

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount Amount cannot be negative");


        }

        private async Task<bool> PurchaseOrderExists(Guid purchaseOrderId, CancellationToken cancellationToken)
        {
            return await _poRepository.AnyAsync(po => po.Id == purchaseOrderId, cancellationToken);
        }

        private async Task<bool> LineItemExists(Guid lineItemId, CancellationToken cancellationToken)
        {
            return await _lineItemRepository.AnyAsync(li => li.Id == lineItemId, cancellationToken);
        }
    }

}

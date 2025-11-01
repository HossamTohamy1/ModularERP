using FluentValidation;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POItme;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Validators.Validators_POItme
{
    public class CreatePOLineItemValidator : AbstractValidator<CreatePOLineItemDto>
    {
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;

        public CreatePOLineItemValidator(IGeneralRepository<PurchaseOrder> poRepository)
        {
            _poRepository = poRepository;

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
    }

}

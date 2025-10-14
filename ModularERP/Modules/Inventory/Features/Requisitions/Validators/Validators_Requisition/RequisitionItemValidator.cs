using FluentValidation;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Validators.Validators_Requisition
{
    public class RequisitionItemValidator : AbstractValidator<CreateRequisitionItemDto>
    {
        private readonly IGeneralRepository<Product> _productRepo;

        public RequisitionItemValidator(IGeneralRepository<Product> productRepo)
        {
            _productRepo = productRepo;

            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product is required.")
                .MustAsync(ProductExists)
                .WithMessage("Product does not exist.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.UnitPrice.HasValue)
                .WithMessage("Unit price cannot be negative.");
        }

        private async Task<bool> ProductExists(Guid productId, CancellationToken cancellationToken)
        {
            return await _productRepo.AnyAsync(p => p.Id == productId);
        }
    }
}


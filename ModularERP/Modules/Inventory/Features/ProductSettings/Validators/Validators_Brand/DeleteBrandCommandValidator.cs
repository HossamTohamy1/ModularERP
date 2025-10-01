using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Brand
{
    public class DeleteBrandCommandValidator : AbstractValidator<DeleteBrandCommand>
    {
        private readonly IGeneralRepository<Brand> _repository;

        public DeleteBrandCommandValidator(IGeneralRepository<Brand> repository)
        {
            _repository = repository;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Brand ID is required")
                .MustAsync(BrandExists).WithMessage("Brand not found");
        }

        private async Task<bool> BrandExists(Guid id, CancellationToken cancellationToken)
        {
            return await _repository.AnyAsync(b => b.Id == id);
        }
    }
}

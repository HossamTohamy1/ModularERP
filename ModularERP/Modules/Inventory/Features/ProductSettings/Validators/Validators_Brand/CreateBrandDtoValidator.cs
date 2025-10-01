using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Brand
{
    public class CreateBrandDtoValidator : AbstractValidator<CreateBrandDto>
    {
        private readonly IGeneralRepository<Brand> _repository;

        public CreateBrandDtoValidator(IGeneralRepository<Brand> repository)
        {
            _repository = repository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Brand name is required")
                .MaximumLength(100).WithMessage("Brand name cannot exceed 100 characters")
                .MustAsync(BeUniqueName).WithMessage("Brand name already exists");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.LogoPath)
                .MaximumLength(500).WithMessage("Logo path cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.LogoPath));
        }

        private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
        {
            return !await _repository.AnyAsync(b => b.Name == name);
        }
    }
}

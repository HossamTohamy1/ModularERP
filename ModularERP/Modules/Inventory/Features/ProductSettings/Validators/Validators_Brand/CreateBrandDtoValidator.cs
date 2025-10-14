using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Brand;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Brand
{
    public class CreateBrandDtoValidator : AbstractValidator<CreateBrandDto>
    {
        public CreateBrandDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Brand name is required")
                .MaximumLength(100).WithMessage("Brand name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.LogoPath)
                .MaximumLength(500).WithMessage("Logo path cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.LogoPath));
        }
    }
}
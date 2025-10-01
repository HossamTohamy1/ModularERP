using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Brand;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Brand
{
    public class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
    {
        public CreateBrandCommandValidator(IValidator<CreateBrandDto> dtoValidator)
        {
            RuleFor(x => x.Dto)
                .NotNull().WithMessage("Brand data is required")
                .SetValidator(dtoValidator);
        }
    }

}

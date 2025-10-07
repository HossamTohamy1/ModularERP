using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Barcode;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Barcode
{
    public class ParseBarcodeValidator : AbstractValidator<ParseBarcodeCommand>
    {
        public ParseBarcodeValidator()
        {
            RuleFor(x => x.Barcode)
                .NotEmpty().WithMessage("Barcode is required")
                .MinimumLength(8).WithMessage("Barcode must be at least 8 characters")
                .MaximumLength(50).WithMessage("Barcode cannot exceed 50 characters");
        }
    }
}

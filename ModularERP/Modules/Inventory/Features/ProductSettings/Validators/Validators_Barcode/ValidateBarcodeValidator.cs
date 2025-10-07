using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Barcode;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Barcode
{
    public class ValidateBarcodeValidator : AbstractValidator<ValidateBarcodeCommand>
    {
        public ValidateBarcodeValidator()
        {
            RuleFor(x => x.Barcode)
                .NotEmpty().WithMessage("Barcode is required")
                .MinimumLength(8).WithMessage("Barcode must be at least 8 characters")
                .MaximumLength(50).WithMessage("Barcode cannot exceed 50 characters");

            RuleFor(x => x.BarcodeType)
                .NotEmpty().WithMessage("Barcode type is required")
                .Must(BeValidBarcodeType).WithMessage("Invalid barcode type");
        }

        private bool BeValidBarcodeType(string barcodeType)
        {
            var validTypes = new[] { "Code128", "EAN13", "UPC", "QRCode", "Code39", "ITF" };
            return validTypes.Contains(barcodeType, StringComparer.OrdinalIgnoreCase);
        }
    }
}
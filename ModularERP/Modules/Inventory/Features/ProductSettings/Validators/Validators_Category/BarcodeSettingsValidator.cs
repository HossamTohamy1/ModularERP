using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Category
{
    public class BarcodeSettingsValidator : AbstractValidator<BarcodeSettings>
    {
        public BarcodeSettingsValidator()
        {
            RuleFor(x => x.BarcodeType)
                .NotEmpty().WithMessage("Barcode type is required")
                .MaximumLength(50).WithMessage("Barcode type cannot exceed 50 characters")
                .Must(x => new[] { "Code128", "EAN13", "UPC", "QRCode", "Code39" }.Contains(x))
                .WithMessage("Invalid barcode type. Allowed: Code128, EAN13, UPC, QRCode, Code39");

            RuleFor(x => x.EmbeddedBarcodeFormat)
                .NotEmpty().WithMessage("Embedded barcode format is required when weight embedded is enabled")
                .MaximumLength(50).WithMessage("Embedded barcode format cannot exceed 50 characters")
                .When(x => x.EnableWeightEmbedded);

            RuleFor(x => x.WeightUnitDivider)
                .GreaterThan(0).WithMessage("Weight unit divider must be greater than 0")
                .When(x => x.EnableWeightEmbedded && x.WeightUnitDivider.HasValue);

            RuleFor(x => x.CurrencyDivider)
                .GreaterThan(0).WithMessage("Currency divider must be greater than 0")
                .When(x => x.CurrencyDivider.HasValue);

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }

}

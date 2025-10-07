using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Barcode;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Barcode
{
    public class CreateBarcodeSettingsValidator : AbstractValidator<CreateBarcodeSettingsCommand>
    {
        public CreateBarcodeSettingsValidator()
        {
            RuleFor(x => x.BarcodeType)
                .NotEmpty().WithMessage("Barcode type is required")
                .MaximumLength(50).WithMessage("Barcode type cannot exceed 50 characters")
                .Must(BeValidBarcodeType).WithMessage("Invalid barcode type. Allowed: Code128, EAN13, UPC, QRCode");

            RuleFor(x => x.EmbeddedBarcodeFormat)
                .MaximumLength(50).WithMessage("Embedded barcode format cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.EmbeddedBarcodeFormat));

            RuleFor(x => x.WeightUnitDivider)
                .GreaterThan(0).WithMessage("Weight unit divider must be greater than 0")
                .When(x => x.WeightUnitDivider.HasValue);

            RuleFor(x => x.CurrencyDivider)
                .GreaterThan(0).WithMessage("Currency divider must be greater than 0")
                .When(x => x.CurrencyDivider.HasValue);

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            RuleFor(x => x.EnableWeightEmbedded)
                .Must((command, enableWeight) =>
                    !enableWeight || !string.IsNullOrEmpty(command.EmbeddedBarcodeFormat))
                .WithMessage("Embedded barcode format is required when weight embedding is enabled");
        }

        private bool BeValidBarcodeType(string barcodeType)
        {
            var validTypes = new[] { "Code128", "EAN13", "UPC", "QRCode", "Code39", "ITF" };
            return validTypes.Contains(barcodeType, StringComparer.OrdinalIgnoreCase);
        }
    }
}

namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Barcode
{
    public class UpdateBarcodeSettingsDto
    {
        public string BarcodeType { get; set; }
        public bool EnableWeightEmbedded { get; set; }
        public string? EmbeddedBarcodeFormat { get; set; }
        public decimal? WeightUnitDivider { get; set; }
        public decimal? CurrencyDivider { get; set; }
        public string? Notes { get; set; }
        public bool IsDefault { get; set; }
    }
}

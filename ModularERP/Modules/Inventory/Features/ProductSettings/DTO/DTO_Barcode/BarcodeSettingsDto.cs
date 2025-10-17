namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Barcode
{
    public class BarcodeSettingsDto
    {
        public Guid Id { get; set; }
        public string BarcodeType { get; set; }
        public bool EnableWeightEmbedded { get; set; }
        public string? EmbeddedBarcodeFormat { get; set; }
        public decimal? WeightUnitDivider { get; set; }
        public decimal? CurrencyDivider { get; set; }
        public string? Notes { get; set; }
        public bool IsDefault { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}

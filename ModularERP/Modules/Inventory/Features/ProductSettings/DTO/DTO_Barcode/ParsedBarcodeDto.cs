namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Barcode
{
    public class ParsedBarcodeDto
    {
        public string OriginalBarcode { get; set; }
        public string ProductCode { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Price { get; set; }
        public string CheckDigit { get; set; }
        public bool IsValid { get; set; }
        public string BarcodeType { get; set; }
    }
}
